#include <iostream>
#include <vector>
#include <string>
#include <atomic>
#include <stdexcept>
#include <type_traits>
#include <thread>
#include <chrono>
#include <intrin.h>
#include <cstring>

#if defined(_WIN32)
#include <windows.h>
#include <synchapi.h>
#pragma comment(lib, "Synchronization")
#endif

#include "VWiring.h"
#include "verilated.h"
#include <verilated_vcd_c.h>

constexpr auto MAX_CYCLE_COUNT = 1000;

constexpr auto SHARED_MEM_NAME = "TerrariaWiringSim_SharedMem";
constexpr auto IPC_MAX_OUTPUT_IDS_PER_SET = 65536;

// constexpr auto LOG_INTERVAL = 1000;

static VWiring *top;

#pragma pack(push, 1)
struct SharedMemoryLayout
{
	volatile int32_t sim_ready;
	volatile int32_t input_ready;
	volatile int32_t output_ready;
	volatile int32_t shutdown;

	int32_t input_id;
	int32_t output_count;
	int32_t output_ids[IPC_MAX_OUTPUT_IDS_PER_SET];
};
#pragma pack(pop)

static HANDLE hMapFile = NULL;
static SharedMemoryLayout *pSharedMem = nullptr;

static thread_local std::vector<int32_t> output_cache;

static uint64_t total_simulations = 0;
static uint64_t total_simulation_time_us = 0;
static uint64_t max_simulation_time_us = 0;

template <typename T>
void clear_input(T &in)
{
	if constexpr (std::is_integral_v<T>)
	{
		in = 0;
	}
	else
	{
		std::memset(&in, 0, sizeof(T));
		// constexpr int nwords = sizeof(T) / sizeof(uint32_t);
		// VL_ZERO_W(nwords, static_cast<WDataOutP>(in));
	}
}

template <typename T>
void set_input_bit(T &in, int input_idx)
{
	if constexpr (std::is_integral_v<T>)
	{
		in = (1ULL << input_idx);
	}
	else
	{
		const int word_index = input_idx / 32;
		const int bit_in_word = input_idx % 32;
		constexpr int words = sizeof(in) / sizeof(uint32_t);
		if (word_index < words)
		{
			in[word_index] = (1U << bit_in_word);
		}
	}
}

template <typename T>
void get_output_bit(const T &out, std::vector<int32_t> &outputs)
{
	if constexpr (std::is_integral_v<T>)
	{
		uint64_t v = static_cast<uint64_t>(out);
		while (v)
		{
			unsigned long bitIdx;
			_BitScanForward64(&bitIdx, v);
			outputs.push_back(static_cast<int32_t>(bitIdx));
			v &= (v - 1);
		}
	}
	else
	{
		const uint32_t *words = reinterpret_cast<const uint32_t *>(&out);
		const int words_count = static_cast<int>(sizeof(out) / sizeof(uint32_t));
		for (int w = 0; w < words_count; ++w)
		{
			uint32_t word = words[w];
			while (word)
			{
				unsigned long bitIdx;
				_BitScanForward(&bitIdx, word);
				outputs.push_back(w * 32 + static_cast<int32_t>(bitIdx));
				word &= (word - 1);
			}
		}
	}
}
/*
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
		constexpr int words = sizeof(out) / sizeof(uint32_t);
		if (word_index >= words)
			return false;
		else
			return ((out[word_index] >> bit_in_word) & 1);
	}
}
*/

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
	std::cout << "[SIM] Performing initial reset...\n";
	top->reset = 1;
	for (int i = 0; i < 5; ++i)
	{
		toggle_clock();
	}
	top->reset = 0;
	top->logic_reset = 0;

	clear_input(top->in);
	toggle_eval();
	std::cout << "[SIM] Initial reset complete.\n";
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
		set_input_bit(top->in, input_idx);
	toggle_clock();
	clear_input(top->in);
	toggle_eval();

	// std::cout << "[SIM] Input pulse sent. Running wiring..." << std::endl;

	int cycle_count = 0;
	// auto start_time = std::chrono::high_resolution_clock::now();
	do
	{
		toggle_clock();
		cycle_count++;
	} while (top->wiring_running != 0 && cycle_count < MAX_CYCLE_COUNT);
	// auto end_time = std::chrono::high_resolution_clock::now();
	// auto duration_us = std::chrono::duration_cast<std::chrono::microseconds>(end_time - start_time).count();

	if (cycle_count >= MAX_CYCLE_COUNT)
	{
		std::cerr << "[SIM] WARNING: Simulation timed out! Wiring may be unstable.\n";
	}
	else
	{
		// total_simulations++;
		// total_simulation_time_us += (uint64_t)duration_us;
		// if ((uint64_t)duration_us > max_simulation_time_us) max_simulation_time_us = (uint64_t)duration_us;
		// if (total_simulations % LOG_INTERVAL == 0)
		// {
		// 	uint64_t avg_us = total_simulation_time_us / total_simulations;
		// 	std::cout << "[SIM] Stats: count=" << total_simulations
		// 		<< ", avg_us=" << avg_us
		// 		<< ", max_us=" << max_simulation_time_us
		// 		<< ", last_cycles=" << cycle_count << "\n";
		// }
	}

	output_cache.clear();
	output_cache.reserve(64);
	get_output_bit(top->out, output_cache);

	return output_cache;
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
	std::cout << "[IPC] Resources cleaned up.\n";
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
			catch (const std::invalid_argument &ia)
			{
				std::cerr << "[Console] Invalid input. Please enter an integer, 'reset', or 'exit'." << std::endl;
				continue;
			}
			catch (const std::out_of_range &oor)
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

	std::cout << "[IPC] Shared memory created. Waiting for client commands...\n";

	int last_seq = -1;
	while (true)
	{
		if (pSharedMem->shutdown != 0)
		{
			std::cout << "[IPC] Shutdown signal received.\n";
			break;
		}

		int expected = 0;
		while (pSharedMem->input_ready == 0 && pSharedMem->shutdown == 0)
		{
			WaitOnAddress((volatile void *)&pSharedMem->input_ready, &expected, sizeof(expected), INFINITE);
		}
		if (pSharedMem->shutdown != 0)
			break;

		int input_id = pSharedMem->input_id;
		pSharedMem->input_ready = 0;

		auto outputs = run_simulation_cycle(input_id);

		int32_t data_len = static_cast<int32_t>(outputs.size());
		if (data_len > IPC_MAX_OUTPUT_IDS_PER_SET)
		{
			std::cerr << "[IPC] ERROR: Output data size (" << data_len
					  << ") exceeds IPC_MAX_OUTPUT_IDS_PER_SET (" << IPC_MAX_OUTPUT_IDS_PER_SET
					  << "). Truncating output.\n";
			data_len = IPC_MAX_OUTPUT_IDS_PER_SET;
		}

		pSharedMem->output_count = data_len;
		if (data_len > 0)
		{
			memcpy(pSharedMem->output_ids, outputs.data(), data_len * sizeof(int32_t));
		}

		pSharedMem->output_ready = 1;
		WakeByAddressAll((PVOID)&pSharedMem->output_ready);
	}

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
