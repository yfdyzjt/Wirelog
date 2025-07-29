module Output_Single (
    input wire clk,            
    input wire logic_reset,         
    input wire in,
    output wire out
);
    
    wire result = in;

    Output_Logic output_logic (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(result),
        .out(out)
    );

endmodule