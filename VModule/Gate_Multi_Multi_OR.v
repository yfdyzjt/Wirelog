module Gate_Multi_Multi_OR #(
    parameter INPUT_COUNT = 2,
    parameter OUTPUT_COUNT = 2
)(        
    input wire clk,
    input wire logic_reset,       
    input wire [INPUT_COUNT-1:0] in, 
    output wire [OUTPUT_COUNT-1:0] out
);

    wire result;

    assign out = {OUTPUT_COUNT{result}};

    Gate_Multi_Single_OR gl (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(in),
        .out(result)
    );

endmodule