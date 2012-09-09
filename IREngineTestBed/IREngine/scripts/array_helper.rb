class Array
  alias to_s_old to_s
  def to_s
    "[ " +
    self.collect {
      |val|
      val.to_s
    }.join("\n") +
    " ]"

  end
  alias inspect_old inspect
  def inspect
    "[ " + self.join("\t") + " ]"
  end
  def inspect_tr
    "[  " +
    self.collect {
      |val|
      val.inspect
    }.join("\n   ") +
    "  ]"
  end
  # Shuffles the array modifying its order.
  def shuffle!
    r = System::Random.new
    self.size.downto(1) { |n| push delete_at(r.rand(n)) }
    self
  end
  
  # Yeilds given bloc using arrays items pair by pair.
  # e.g.
  # <code>
  # ["a","b","c","d"].each_pair do |first, second|
  #  puts second + " - " + second
  # end
  # </code>
  # will print:
  # b - a
  # c - d
  # 
  def each_pair
    num = self.size/2
    (0..num-1).collect do |index|
      yield self[index*2], self[(index*2)+1]
    end
  end
  
  # Splits the array into two parts first from position
  # 0 to "position" and second from position "position+1" to
  # last position.
  # Returns two new arrays.
  def separate(position)
   return self[0..position], self[position+1..-1]
  end
  
end
