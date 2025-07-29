module Wiring #(
    parameter INPUT_WIDTH = 1,
    parameter OUTPUT_WIDTH = 2
)(
    input wire clk,
    input wire reset,
    input wire logic_reset,
    input wire [INPUT_WIDTH-1:0] in,
    output wire wiring_running,
    output wire [OUTPUT_WIDTH-1:0] out,
    output wire [31:0] in_width,
    output wire [31:0] out_width
);

    parameter GATE_WIRES_WIDTH = 2;
    parameter LAMP_WIRES_WIDTH = 4;

    wire [GATE_WIRES_WIDTH-1:0] gate_wires;
    wire [LAMP_WIRES_WIDTH-1:0] lamp_wires;

    Lamp_Single_On lamp_single_on_1 (
        .clk(clk),
        .reset(reset),
        .in(in[0]),
        .out(lamp_wires[0])
    );

    Lamp_Single_Fault lamp_single_fault_1 (
        .clk(clk),
        .in(in[0]),
        .out(lamp_wires[1])
    );

    Lamp_Single_On lamp_single_on_2 (
        .clk(clk),
        .reset(reset),
        .in(gate_wires[0]),
        .out(lamp_wires[2])
    );

    Lamp_Single_Fault lamp_single_fault_2 (
        .clk(clk),
        .in(gate_wires[0]),
        .out(lamp_wires[3])
    );

    Gate_Single_Fault gate_single_fault_1 (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(lamp_wires[0]),
        .fault_in(lamp_wires[1]),
        .out(gate_wires[0])
    );

    Gate_Single_Fault gate_single_fault_2 (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(lamp_wires[2]),
        .fault_in(lamp_wires[3]),
        .out(gate_wires[1])
    );

    Output_Single output_single_1 (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(gate_wires[0]),
        .out(out[0])
    );

    Output_Single output_single_2 (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(gate_wires[1]),
        .out(out[1])
    );

    assign wiring_running = |{in, gate_wires};

    assign in_width = INPUT_WIDTH;
    assign out_width = OUTPUT_WIDTH;

endmodule