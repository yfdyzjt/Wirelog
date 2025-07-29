module Lamp_Single_On (
    input wire clk,            
    input wire reset,         
    input wire in,
    output wire out
);
    
    wire result = in;

    Lamp_Logic #( .INIT_VALUE(1) ) lamp_logic (
        .clk(clk),
        .reset(reset),
        .in(result),
        .out(out)
    );

endmodule