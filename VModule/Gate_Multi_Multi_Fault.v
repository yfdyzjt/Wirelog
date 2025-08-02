module Gate_Multi_Multi_Fault #(
    parameter INPUT_COUNT = 2,
    parameter OUTPUT_COUNT = 2,
    parameter RAND_SEED = 12'hAAA
)(
    input wire clk,
    input wire reset,
    input wire logic_reset,       
    input wire [INPUT_COUNT-1:0] in, 
    input wire fault_in, 
    output wire [OUTPUT_COUNT-1:0] out
);

    wire result;

    assign out = {OUTPUT_COUNT{result}};

    Gate_Single_Multi_Fault #( .INPUT_COUNT(INPUT_COUNT), .RAND_SEED(RAND_SEED) ) gl (
        .clk(clk),
        .reset(reset),
        .logic_reset(logic_reset),
        .in(in),
        .fault_in(fault_in),
        .out(result)
    );

endmodule