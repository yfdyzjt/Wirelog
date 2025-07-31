module Lamp_Multi_Off #(
    parameter INPUT_COUNT = 2  
)(
    input wire clk,            
    input wire reset,         
    input wire [INPUT_COUNT-1:0] in,
    output wire out
);
    
    wire result = ^in;

    Lamp_Logic #( .INIT_VALUE(0) ) ll (
        .clk(clk),
        .reset(reset),
        .in(result),
        .out(out)
    );

endmodule