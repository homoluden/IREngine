class System::Random
  def rand(val = 1.0)
    if val.class == Range
      min,max = val.begin.to_f, val.end.to_f
      delta = max - min
      return delta*self.NextDouble + min
    end
    return self.Next(val) if val.integer?
    
    val*self.NextDouble
  end
  
end