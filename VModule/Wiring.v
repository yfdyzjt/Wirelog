module Wiring #(
    parameter INPUT_WIDTH = 2,
    parameter OUTPUT_WIDTH = 1
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

    parameter WIRES_COUNT = 3;
    parameter LAMPS_COUNT = 2;

    wire [WIRES_COUNT-1:0] wires;
    wire [LAMPS_COUNT-1:0] lamps;

    assign wiring_running = |wires;

    assign in_width = INPUT_WIDTH;
    assign out_width = OUTPUT_WIDTH;

    Input_Single input_0 (
        .in(in[0]),
        .out(wires[0])
    );

    Input_Single input_1 (
        .in(in[1]),
        .out(wires[1])
    );

    Lamp_Single_On lamp_0 (
        .clk(clk),
        .reset(reset),
        .in(wires[0]),
        .out(lamps[0])
    );

    Lamp_Single_On lamp_1 (
        .clk(clk),
        .reset(reset),
        .in(wires[1]),
        .out(lamps[1])
    );

    Gate_Multi_Single_AND #( .INPUT_COUNT(2) ) gate_0 (
        .clk(clk),
        .logic_reset(logic_reset),
        .in({lamps[1], lamps[0]}),
        .out(wires[2])
    );

    Output_Single output_0 (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(wires[2]),
        .out(out[0])
    );

endmodule