module Lamp_Single_Fault (
    input wire clk,          
    input wire in, 
    output wire out  
);
    
    wire result = in;

    Lamp_Fault_Logic lamp_fault_logic (
        .clk(clk),
        .in(result),
        .out(out)
    );

endmodule