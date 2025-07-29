module Lamp_Multi_Fault #(
    parameter INPUT_COUNT = 2
)(
    input wire clk,          
    input wire [INPUT_COUNT-1:0] in, 
    output wire out  
);
    
    wire result = |in;

    Lamp_Fault_Logic lamp_fault_logic (
        .clk(clk),
        .in(result),
        .out(out)
    );

endmodule