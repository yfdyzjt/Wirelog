module Gate_Single_Single (        
    input wire clk,
    input wire logic_reset,       
    input wire in, 
    output wire out
);

    Gate_Logic gl (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(in),
        .out(out)
    );

endmodule