#include <iostream>
#include <vector>
#include <string>
#include <atomic>
#include <stdexcept>
#include <type_traits>
#include <thread>
#include <chrono>

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

#define IPC_MAX_OUTPUT_IDS_PER_SET 65536

static VWiring* top;

#pragma pack(push, 1)
struct SharedMemoryLayout
{
	std::atomic<int32_t> sim_ready;
	std::atomic<int32_t> input_ready;
	std::atomic<int32_t> output_ready;
	std::atomic<int32_t> shutdown;

	int32_t input_id;
	int32_t output_count;
	int32_t output_ids[IPC_MAX_OUTPUT_IDS_PER_SET];
};
#pragma pack(pop)

static HANDLE hMapFile = NULL;
static SharedMemoryLayout* pSharedMem = nullptr;
static HANDLE hInputEvent = NULL;
static HANDLE hOutputEvent = NULL;
static HANDLE hShutdownEvent = NULL;

template <typename T>
void clear_input(T& in)
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
void set_input_bit(T& in, int input_idx)
{
	if constexpr (std::is_integral_v<T>)
	{
		in = (1ULL << input_idx);
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
bool get_output_bit(const T& out, int bit_idx)
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

void initial_reset()
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
}

std::vector<int32_t> run_simulation_cycle(int input_idx)
{
	if (input_idx < 0)
	{
		std::cout << "[SIM] Received external reset command." << std::endl;
		top->reset = 1;
		for (int i = 0; i < 5; ++i)
			toggle_clock();
		top->reset = 0;
		toggle_eval();
		return {};
	}

	// std::cout << "[SIM] Processing new input: " << input_idx << std::endl;

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

	// std::cout << "[SIM] Input pulse sent. Running wiring..." << std::endl;

	int cycle_count = 0;
	auto start_time = std::chrono::high_resolution_clock::now();
	do
	{
		toggle_clock();
		cycle_count++;
	} while (top->wiring_running != 0 && cycle_count < MAX_CYCLE_COUNT);
	auto end_time = std::chrono::high_resolution_clock::now();
	auto duration_us = std::chrono::duration_cast<std::chrono::microseconds>(end_time - start_time).count();

	if (cycle_count >= MAX_CYCLE_COUNT)
	{
		std::cerr << "[SIM] WARNING: Simulation timed out! Wiring may be unstable." << std::endl;
	}
	else
	{
		std::cout << "[SIM] Wiring stable after " << cycle_count << " cycles, " 
			<< "duration: " << duration_us << " us." << std::endl;
	}

	std::vector<int32_t> output_idx;
	for (int i = 0; i < top->out_width; ++i)
	{
		if (get_output_bit(top->out, i))
		{
			output_idx.push_back(i);
		}
	}
	// std::cout << "[SIM] Found " << output_idx.size() << " active outputs." << std::endl;
	return output_idx;
}

void cleanup_ipc()
{
	if (pSharedMem)
	{
		pSharedMem->sim_ready = 0;
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

	pSharedMem = (SharedMemoryLayout*)MapViewOfFile(
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
	pSharedMem->input_ready = 0;
	pSharedMem->output_ready = 0;
	pSharedMem->shutdown = 0;
	pSharedMem->sim_ready = 1;

	return true;
}

void run_console_mode()
{
	std::cout << "[Console] Entered console mode. Type an input index (e.g., 5) and press Enter." << std::endl;
	std::cout << "[Console] Type 'r(eset)' to reset the circuit, or 'e(xit)' to quit." << std::endl;

	std::string line;
	while (std::getline(std::cin, line))
	{
		if (line == "exit" || line == "e")
		{
			break;
		}

		int input_id;
		if (line == "reset" || line == "r")
		{
			input_id = -1;
		}
		else
		{
			try
			{
				input_id = std::stoi(line);
			}
			catch (const std::invalid_argument& ia)
			{
				std::cerr << "[Console] Invalid input. Please enter an integer, 'reset', or 'exit'." << std::endl;
				continue;
			}
			catch (const std::out_of_range& oor)
			{
				std::cerr << "[Console] Input number is out of range." << std::endl;
				continue;
			}
		}

		auto output_idx = run_simulation_cycle(input_id);

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
	std::cout << "[Console] Exiting console mode." << std::endl;
}

void run_ipc_mode()
{
	std::cout << "[Main] Starting in IPC Mode." << std::endl;
	if (!initialize_ipc())
	{
		return;
	}

	std::cout << "[IPC] Shared memory and events created. Waiting for client commands..." << std::endl;

	HANDLE handles[] = { hInputEvent, hShutdownEvent };

	while (true)
	{
		DWORD waitResult = WaitForMultipleObjects(2, handles, FALSE, INFINITE);

		if (waitResult == WAIT_OBJECT_0 + 1 || pSharedMem->shutdown != 0)
		{
			std::cout << "[IPC] Shutdown signal received." << std::endl;
			break;
		}

		if (waitResult == WAIT_OBJECT_0)
		{
			if (pSharedMem->input_ready == 1)
			{
				int input_id = pSharedMem->input_id;
				pSharedMem->input_ready = 0;

				auto outputs = run_simulation_cycle(input_id);

				int32_t data_len = static_cast<int32_t>(outputs.size());
				if (data_len > IPC_MAX_OUTPUT_IDS_PER_SET)
				{
					std::cerr << "[IPC] ERROR: Output data size (" << data_len
						<< ") exceeds IPC_MAX_OUTPUT_IDS_PER_SET (" << IPC_MAX_OUTPUT_IDS_PER_SET
						<< "). Truncating output." << std::endl;
					data_len = IPC_MAX_OUTPUT_IDS_PER_SET;
				}

				pSharedMem->output_count = data_len;
				if (data_len > 0)
				{
					memcpy(pSharedMem->output_ids, outputs.data(), data_len * sizeof(int32_t));
				}

				pSharedMem->output_ready = 1;
				SetEvent(hOutputEvent);
			}
		}
	}

	cleanup_ipc();
}

int main(int argc, char** argv)
{
	std::string mode = "console";
	if (argc > 1 && std::string(argv[1]) == "--ipc")
	{
		mode = "ipc";
	}

	Verilated::commandArgs(argc, argv);
	top = new VWiring;
	
	initial_reset();

	std::cout << "[Main] Verilator core initialized." << std::endl;

	if (mode == "ipc")
	{
		run_ipc_mode();
	}
	else
	{
		run_console_mode();
	}

	delete top;
	top = nullptr;
	std::cout << "[Main] Application finished." << std::endl;

	return 0;
}
