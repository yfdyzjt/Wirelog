#include <iostream>
#include <queue>
#include <mutex>
#include <thread>
#include <cstring>
#include <string>
#include <vector>
#include <atomic>
#include <stdexcept>
#include <type_traits>
#include <condition_variable>

#if defined(_WIN32)
#include <windows.h>
#endif

#include "VWiring.h"
#include "verilated.h"
#include <verilated_vcd_c.h>

#define MAX_CYCLE_COUNT 1000

#define SHARED_MEM_NAME "TerrariaWiringSim_SharedMem"
#define INPUT_EVENT_NAME "TerrariaWiringSim_InputEvent"
#define OUTPUT_EVENT_NAME "TerrariaWiringSim_OutputEvent"
#define SHUTDOWN_EVENT_NAME "TerrariaWiringSim_ShutdownEvent"

#define IPC_INPUT_BUFFER_SIZE 8192
#define IPC_MAX_OUTPUT_IDS_PER_SET 65536
#define IPC_MAX_OUTPUT_SETS 1024

static VWiring *top;

static std::thread sim_thread;
static std::mutex mtx;
static std::condition_variable cv_input;
static std::condition_variable cv_output;
static bool stop_thread = false;

static std::queue<int> input_queue;
static std::queue<std::vector<int32_t>> output_queue;

#pragma pack(push, 1)
struct OutputSet
{
    int32_t count;
    int32_t ids[IPC_MAX_OUTPUT_IDS_PER_SET];
};

struct SharedMemoryLayout
{
    std::atomic<bool> sim_ready;

    std::atomic<uint64_t> input_write_idx;
    std::atomic<uint64_t> input_read_idx;
    int32_t input_buffer[IPC_INPUT_BUFFER_SIZE];

    std::atomic<uint64_t> output_write_idx;
    std::atomic<uint64_t> output_read_idx;
    OutputSet output_sets[IPC_MAX_OUTPUT_SETS];
};
#pragma pack(pop)

static HANDLE hMapFile = NULL;
static SharedMemoryLayout *pSharedMem = nullptr;
static HANDLE hInputEvent = NULL;
static HANDLE hOutputEvent = NULL;
static HANDLE hShutdownEvent = NULL;

template <typename T>
void clear_input(T &in)
{
    if constexpr (std::is_integral_v<T>)
    {
        in = 0;
    }
    else
    {
        constexpr int nwords = sizeof(T) / sizeof(uint32_t);
        VL_ZERO_W(nwords, static_cast<WDataOutP>(in));
    }
}

template <typename T>
void set_input_bit(T &in, int input_idx)
{
    if constexpr (std::is_integral_v<T>)
    {
        in = (1U << input_idx);
    }
    else
    {
        const int word_index = input_idx / 32;
        const int bit_in_word = input_idx % 32;
        int words = sizeof(in) / sizeof(uint32_t);
        if (word_index < words)
        {
            in[word_index] = (1U << bit_in_word);
        }
    }
}

template <typename T>
bool get_output_bit(const T &out, int bit_idx)
{
    if constexpr (std::is_integral_v<T>)
    {
        return ((out >> bit_idx) & 1);
    }
    else
    {
        const int word_index = bit_idx / 32;
        const int bit_in_word = bit_idx % 32;
        int words = sizeof(out) / sizeof(uint32_t);
        if (word_index >= words)
            return false;
        else
            return ((out[word_index] >> bit_in_word) & 1);
    }
}

void toggle_clock()
{
    top->clk = 0;
    top->eval();
    top->clk = 1;
    top->eval();
}

void toggle_eval()
{
    top->eval();
}

void simulation_loop()
{
    std::cout << "[SIM] Performing initial reset..." << std::endl;
    top->reset = 1;
    for (int i = 0; i < 5; ++i)
    {
        toggle_clock();
    }
    top->reset = 0;
    top->logic_reset = 0;

    clear_input(top->in);
    toggle_eval();

    std::cout << "[SIM] Initial reset complete." << std::endl;

    while (true)
    {
        int input_idx;
        {
            std::unique_lock<std::mutex> lock(mtx);
            cv_input.wait(lock, []
                          { return !input_queue.empty() || stop_thread; });

            if (stop_thread)
            {
                std::cout << "[SIM] Shutdown signal received. Exiting simulation loop." << std::endl;
                break;
            }

            input_idx = input_queue.front();
            input_queue.pop();
        }

        if (input_idx < 0)
        {
            std::cout << "[SIM] Received external reset command." << std::endl;
            top->reset = 1;
            for (int i = 0; i < 5; ++i)
                toggle_clock();
            top->reset = 0;
            toggle_eval();
            continue;
        }

        std::cout << "[SIM] Processing new input: " << input_idx << std::endl;

        top->logic_reset = 1;
        toggle_clock();
        top->logic_reset = 0;
        toggle_eval();

        if (input_idx >= 0 && input_idx < top->in_width)
        {
            set_input_bit(top->in, input_idx);
        }
        toggle_clock();

        clear_input(top->in);
        toggle_eval();

        std::cout << "[SIM] Input pulse sent. Running wiring..." << std::endl;

        int cycle_count = 0;
        do
        {
            toggle_clock();
            cycle_count++;
        } while (top->wiring_running != 0 && cycle_count < MAX_CYCLE_COUNT);

        if (cycle_count >= MAX_CYCLE_COUNT)
        {
            std::cerr << "[SIM] WARNING: Simulation timed out! Wiring may be unstable." << std::endl;
        }
        else
        {
            std::cout << "[SIM] Wiring stable after " << cycle_count << " cycles." << std::endl;
        }

        std::vector<int32_t> output_idx;
        for (int i = 0; i < top->out_width; ++i)
        {
            if (get_output_bit(top->out, i))
            {
                output_idx.push_back(i);
            }
        }

        {
            std::lock_guard<std::mutex> lock(mtx);
            output_queue.push(output_idx);
        }
        cv_output.notify_one();
    }
}

#if defined(_WIN32)
#define API_EXPORT __declspec(dllexport)
#else
#define API_EXPORT __attribute__((visibility("default")))
#endif

/*
extern "C"
{

    API_EXPORT void sim_init()
    {
        Verilated::commandArgs(0, (const char **)nullptr);
        top = new VWiring;
        stop_thread = false;
        sim_thread = std::thread(simulation_loop);
        std::cout << "[API] Simulation initialized." << std::endl;
    }

    API_EXPORT void sim_shutdown()
    {
        if (stop_thread)
            return;
        {
            std::lock_guard<std::mutex> lock(mtx);
            stop_thread = true;
        }
        cv_input.notify_one();
        if (sim_thread.joinable())
            sim_thread.join();
        delete top;
        top = nullptr;
        std::cout << "[API] Simulation shut down." << std::endl;
    }

    API_EXPORT void sim_send_input(int input_idx)
    {
        {
            std::lock_guard<std::mutex> lock(mtx);
            input_queue.push(input_idx);
        }
        cv_input.notify_one();
    }

    API_EXPORT void sim_send_reset()
    {
        {
            std::lock_guard<std::mutex> lock(mtx);
            input_queue.push(-1);
        }
        cv_input.notify_one();
    }

    API_EXPORT bool sim_is_output_available()
    {
        std::lock_guard<std::mutex> lock(mtx);
        return !output_queue.empty();
    }

    API_EXPORT int sim_get_output_count()
    {
        std::lock_guard<std::mutex> lock(mtx);
        if (output_queue.empty())
        {
            return -1;
        }
        return output_queue.front().size();
    }

    API_EXPORT void sim_get_outputs(int *buffer, int buffer_size)
    {
        std::lock_guard<std::mutex> lock(mtx);
        if (output_queue.empty())
        {
            return;
        }

        const auto &front_vec = output_queue.front();
        int items_to_copy = std::min((int)front_vec.size(), buffer_size);

        for (int i = 0; i < items_to_copy; ++i)
        {
            buffer[i] = front_vec[i];
        }

        output_queue.pop();
    }
}
*/

void signal_threads_to_stop()
{
    {
        std::lock_guard<std::mutex> lock(mtx);
        stop_thread = true;
    }
    cv_input.notify_all();
    cv_output.notify_all();
}

void cleanup_ipc()
{
    if (pSharedMem)
    {
        pSharedMem->sim_ready = false;
        UnmapViewOfFile(pSharedMem);
        pSharedMem = nullptr;
    }
    if (hMapFile)
        CloseHandle(hMapFile);
    if (hInputEvent)
        CloseHandle(hInputEvent);
    if (hOutputEvent)
        CloseHandle(hOutputEvent);
    if (hShutdownEvent)
        CloseHandle(hShutdownEvent);
    std::cout << "[IPC] Resources cleaned up." << std::endl;
}

void console_input_thread()
{
    std::cout << "[Console] Entered console mode. Type an input index (e.g., 5) and press Enter." << std::endl;
    std::cout << "[Console] Type 'r(eset)' to reset the circuit, or 'e(xit)' to quit." << std::endl;

    std::string line;
    while (std::getline(std::cin, line))
    {
        if (line == "exit" || line == "e")
        {
            signal_threads_to_stop();
            break;
        }
        else if (line == "reset" || line == "r")
        {
            {
                std::lock_guard<std::mutex> lock(mtx);
                input_queue.push(-1);
            }
            cv_input.notify_one();
            std::cout << "[Console] Reset signal sent." << std::endl;
        }
        else
        {
            try
            {
                int input_idx = std::stoi(line);
                {
                    std::lock_guard<std::mutex> lock(mtx);
                    input_queue.push(input_idx);
                }
                cv_input.notify_one();
            }
            catch (const std::invalid_argument &ia)
            {
                std::cerr << "[Console] Invalid input. Please enter an integer, 'reset', or 'exit'." << std::endl;
            }
            catch (const std::out_of_range &oor)
            {
                std::cerr << "[Console] Input number is out of range." << std::endl;
            }
        }
    }
    std::cout << "[Console] Input thread finished." << std::endl;
}

void console_output_thread()
{
    while (!stop_thread)
    {
        std::vector<int32_t> output_idx;
        {
            std::unique_lock<std::mutex> lock(mtx);
            cv_output.wait(lock, []
                           { return !output_queue.empty() || stop_thread; });

            if (stop_thread)
                break;

            output_idx = output_queue.front();
            output_queue.pop();
        }

        std::cout << "[Output] Detected " << output_idx.size() << " active output(s): ";
        if (output_idx.empty())
        {
            std::cout << "None" << std::endl;
        }
        else
        {
            for (size_t i = 0; i < output_idx.size(); ++i)
            {
                std::cout << output_idx[i] << (i == output_idx.size() - 1 ? "" : ", ");
            }
            std::cout << std::endl;
        }
    }
    std::cout << "[Console] Output thread finished." << std::endl;
}

void ipc_input_thread()
{
    while (!stop_thread)
    {
        HANDLE handles[] = {hInputEvent, hShutdownEvent};
        DWORD waitResult = WaitForMultipleObjects(2, handles, FALSE, INFINITE);

        if (waitResult == WAIT_OBJECT_0 + 1 || stop_thread)
        {
            break;
        }

        uint64_t read_idx = pSharedMem->input_read_idx.load(std::memory_order_acquire);
        uint64_t write_idx = pSharedMem->input_write_idx.load(std::memory_order_acquire);

        while (read_idx != write_idx)
        {
            int input_val = pSharedMem->input_buffer[read_idx & (IPC_INPUT_BUFFER_SIZE - 1)];
            {
                std::lock_guard<std::mutex> lock(mtx);
                input_queue.push(input_val);
            }
            read_idx++;
        }

        pSharedMem->input_read_idx.store(read_idx, std::memory_order_release);
        cv_input.notify_one();
    }
    std::cout << "[IPC] Input thread finished." << std::endl;
}

void ipc_output_thread()
{
    while (!stop_thread)
    {
        std::vector<int32_t> output_data;
        {
            std::unique_lock<std::mutex> lock(mtx);
            cv_output.wait(lock, [&]
                           { return !output_queue.empty() || stop_thread; });
            if (stop_thread)
                break;
            output_data = output_queue.front();
            output_queue.pop();
        }

        uint64_t write_idx = pSharedMem->output_write_idx.load(std::memory_order_acquire);
        uint64_t read_idx = pSharedMem->output_read_idx.load(std::memory_order_acquire);

        while (((write_idx + 1) & (IPC_MAX_OUTPUT_SETS - 1)) == (read_idx & (IPC_MAX_OUTPUT_SETS - 1)))
        {
            if (stop_thread)
                return;
            std::this_thread::sleep_for(std::chrono::microseconds(10));
            read_idx = pSharedMem->output_read_idx.load(std::memory_order_acquire);
        }

        uint64_t current_write_idx = write_idx & (IPC_MAX_OUTPUT_SETS - 1);
        OutputSet &target_set = pSharedMem->output_sets[current_write_idx];

        int32_t data_len = static_cast<int32_t>(output_data.size());
        if (data_len > IPC_MAX_OUTPUT_IDS_PER_SET)
        {
            std::cerr << "[IPC] ERROR: Output data size (" << data_len
                      << ") exceeds IPC_MAX_OUTPUT_IDS_PER_SET (" << IPC_MAX_OUTPUT_IDS_PER_SET
                      << "). Truncating output." << std::endl;
            data_len = IPC_MAX_OUTPUT_IDS_PER_SET;
        }

        target_set.count = data_len;
        if (data_len > 0)
        {
            memcpy(target_set.ids, output_data.data(), data_len * sizeof(int32_t));
        }

        pSharedMem->output_write_idx.store(write_idx + 1, std::memory_order_release);

        SetEvent(hOutputEvent);
    }
    std::cout << "[IPC] Output thread finished." << std::endl;
}

bool initialize_ipc()
{
    hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,
        NULL,
        PAGE_READWRITE,
        0,
        sizeof(SharedMemoryLayout),
        SHARED_MEM_NAME);

    if (hMapFile == NULL)
    {
        std::cerr << "[IPC] Failed to create shared memory file mapping object: " << GetLastError() << std::endl;
        return false;
    }

    pSharedMem = (SharedMemoryLayout *)MapViewOfFile(
        hMapFile,
        FILE_MAP_ALL_ACCESS,
        0, 0,
        sizeof(SharedMemoryLayout));

    if (pSharedMem == NULL)
    {
        std::cerr << "[IPC] Failed to map view of shared memory: " << GetLastError() << std::endl;
        cleanup_ipc();
        return false;
    }

    hInputEvent = CreateEvent(NULL, FALSE, FALSE, INPUT_EVENT_NAME);
    hOutputEvent = CreateEvent(NULL, FALSE, FALSE, OUTPUT_EVENT_NAME);
    hShutdownEvent = CreateEvent(NULL, TRUE, FALSE, SHUTDOWN_EVENT_NAME);

    if (!hInputEvent || !hOutputEvent || !hShutdownEvent)
    {
        std::cerr << "[IPC] Failed to create sync events: " << GetLastError() << std::endl;
        cleanup_ipc();
        return false;
    }

    new (pSharedMem) SharedMemoryLayout();
    pSharedMem->sim_ready = true;

    return true;
}

void run_console_mode()
{
    std::cout << "[Main] Starting in Console Mode." << std::endl;
    std::thread console_in(console_input_thread);
    std::thread console_out(console_output_thread);

    console_in.join();
    console_out.join();
}

void run_ipc_mode()
{
    std::cout << "[Main] Starting in IPC Mode." << std::endl;
    if (!initialize_ipc())
    {
        return;
    }

    std::cout << "[IPC] Shared memory and events created. Waiting for client connection..." << std::endl;

    std::thread ipc_in(ipc_input_thread);
    std::thread ipc_out(ipc_output_thread);

    WaitForSingleObject(hShutdownEvent, INFINITE);
    std::cout << "[Main] Received IPC shutdown signal." << std::endl;

    signal_threads_to_stop();

    ipc_in.join();
    ipc_out.join();

    cleanup_ipc();
}

int main(int argc, char **argv)
{
    std::string mode = "console";
    if (argc > 1 && std::string(argv[1]) == "--ipc")
    {
        mode = "ipc";
    }

    Verilated::commandArgs(argc, argv);
    top = new VWiring;
    stop_thread = false;
    sim_thread = std::thread(simulation_loop);
    std::cout << "[Main] Verilator core initialized, simulation thread started." << std::endl;

    if (mode == "ipc")
    {
        run_ipc_mode();
    }
    else
    {
        run_console_mode();
    }

    std::cout << "[Main] Waiting for simulation thread to shut down..." << std::endl;
    if (sim_thread.joinable())
    {
        sim_thread.join();
    }
    delete top;
    top = nullptr;
    std::cout << "[Main] Application finished." << std::endl;

    return 0;
}
