architecture Behavioural of CustomWrapper is
  signal Trigger : std_logic := '0'; -- 1 when risign edge detected
  signal Previous : std_logic := '0'; -- 1 if last edge was rising, 0 if last edge was falling
  signal TriggerCounter : signed(15 downto 0); -- counts number of periods 
  signal TriggerLimit : signed(15 downto 0); -- if TriggerCounter reached TriggerLimit, counted clock cycles will be saved and output
  signal HIThreshold : signed(15 downto 0); -- threshold when to detect rising edge, roughly in mV
  signal LOThreshold : signed(15 downto 0); -- threshold when to detect falling edge, roughly in mV

  signal ClkCounter : signed(15 downto 0);-- counts the clock cycles
  signal ClkDivided : signed(15 downto 0);-- counts overflows of ClkCounter, this will overflow after ~13 s (4294967295 / (312.5 MHz))

  signal ClockHold : signed(15 downto 0);-- holds value of ClkCounter until next TriggerLimit was reached
  signal DividerHold : signed(15 downto 0); -- holds value of ClkDivided until next TriggerLimit was reached

  constant HI_LVL : signed(15 downto 0) := x"7FFF";
  constant LO_LVL : signed(15 downto 0) := x"0000";
begin

  HIThreshold <= signed(Control0(15 downto 0)); -- threshold when to detect rising edge, roughly in mV
  LOThreshold <= signed(Control1(15 downto 0)); -- threshold when to detect falling edge, roughly in mV
  TriggerLimit <= signed(Control2(15 downto 0)); -- number of periods to count

  RisingEdgeTrigger: process(Clk) is -- set Trigger for one clock cycle when there is a rising edge
  begin
    if rising_edge(Clk) then
      if InputA >= HIThreshold*to_signed(2,16) and Previous = '0' then -- 2270.02 bits per volt ~ 440 µV / bit. *2 -> input unit of threshold is roughly mV
        Trigger <= '1';
        Previous <= '1';
      elsif InputA < -LOThreshold*to_signed(2,16) and Previous = '1' then
        Trigger <= '0';
        Previous <= '0';
      else
        Trigger <= '0';
      end if;
    end if;
  end process;

  TriggerCounting: process(Clk) is
  begin
    if rising_edge(Clk) then
      if Reset = '1' then
        ClkCounter <= (others =>'0');
        ClkDivided <= (others =>'0');
        TriggerCounter <= (others =>'0');
        ClockHold <= (others =>'0');
        DividerHold <= (others =>'0');
      else
        ClkCounter <= ClkCounter + to_signed(1,16);
        if ClkCounter = x"FFFF" then
          ClkDivided <= ClkDivided + to_signed(1,16);
        end if;
        
        if Trigger = '0' and TriggerCounter = to_signed(0,16) then -- wait for first rising edge
          ClkCounter <= (others =>'0');
          ClkDivided <= (others =>'0');
        elsif Trigger = '1' then
          TriggerCounter <= TriggerCounter + to_signed(1,16);
          if TriggerCounter = TriggerLimit  then -- +1 because TriggerCounter only gets updated at the end of process but -1 because first trigger is already counted as 1 period
            ClkCounter <= (others =>'0');
            ClkDivided <= (others =>'0');
            TriggerCounter <= (others =>'0');
            ClockHold <= ClkCounter; 
            DividerHold <= ClkDivided;
          end if;
        end if;
      end if;
    end if;
  end process;

  OutputA <= ClockHold; -- 29925 bits/Volt, 312.5 MHz clock rate, measured frequency = 1/(((OutputA Volt + 65535 * OutputB Volt) * 29925 bits/Volt)/(312.5*10^6 bits/s)/(Period count))
  OutputB <= DividerHold;
--OutputB <= HI_LVL when Previous = '1' else LO_LVL;
end architecture;