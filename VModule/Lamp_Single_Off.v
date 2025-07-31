module Lamp_Single_Off (
    input wire clk,            
    input wire reset,         
    input wire in,
    output wire out
);
    
    wire result = in;

    Lamp_Logic #( .INIT_VALUE(0) ) ll (
        .clk(clk),
        .reset(reset),
        .in(result),
        .out(out)
    );

endmodule