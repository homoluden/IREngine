#!ruby19
# encoding: utf-8
$: << File.expand_path(File.dirname(__FILE__))
$: << "#{File.expand_path(File.expand_path(File.dirname(__FILE__)))}/.."
def require_relative path
    require "#{File.expand_path(File.dirname(__FILE__))}/#{path}"
end

$verbose = false

require "IREngine.dll"
require "IRGame.dll"

require "array_helper"
require "random_helper"

include IREngine
include IRGame::Common

class Vector2

    def Vector2.from_array array
        result = Vector2.new
        result.x, result.y = array[0], array[1]
        
        result
    end

end

class Float
  def to_s
    return "%.2e" % self if self.abs > 1e+4
    return "%.4f" % self if self.abs > 1e-4
    "%.2e" % self
  end
  def to_deg
    self * 180 / Math.const_get(:PI)
  end
end

class Vector2
    def angle
        Vector2Ex.angle self
    end
end