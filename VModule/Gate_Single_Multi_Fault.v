module Gate_Single_Multi_Fault #(
    parameter OUTPUT_COUNT = 2
)(
    input wire clk,
    input wire logic_reset,       
    input wire in, 
    input wire fault_in, 
    output wire [OUTPUT_COUNT-1:0] out
);

    wire result;

    assign out = {OUTPUT_COUNT{result}};

    Gate_Single_Single_Fault gl (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(in),
        .fault_in(fault_in),
        .out(result)
    );

endmodule