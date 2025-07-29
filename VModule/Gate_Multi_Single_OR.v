module Gate_Multi_Single_OR #(
    parameter INPUT_COUNT = 2
)(        
    input wire clk,
    input wire logic_reset,       
    input wire [INPUT_COUNT-1:0] in, 
    output wire out
);

    wire result = |in;

    Gate_Logic gl (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(result),
        .out(out)
    );

endmodule