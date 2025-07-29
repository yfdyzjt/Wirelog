module Gate_Single_Single_Fault (
    input wire clk,
    input wire logic_reset,       
    input wire in, 
    input wire fault_in, 
    output wire out
);

    wire result = fault_in & in;

    Gate_Fault_Logic gl (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(result),
        .out(out)
    );

endmodule