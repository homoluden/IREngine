#!ruby19
# encoding: utf-8
require "helpers"
require "logging"
IRE.log "Character Test: after requires" if $verbose
$: << File.expand_path(File.expand_path(File.dirname(__FILE__)))
IRE.log $:.to_s if $verbose

require_relative "game/character"

include System

ts = 0.001
ch = Character.build_discrete_model [0.1, 0.05, 1.0], [0.1, 0.09, 1.0], [0.01, 0.1], [0.01, 0.1], ts
IRE.log Character.print ch.state

ch.set_target_position 1.5, 3.0

#IRE.log "Trace 1"
time = 0.0
last = Environment.tick_count
50000.times{|i|
    curr = Environment.tick_count
    delta = (curr - last) * 0.001
    time += delta
    IRE.log "Time: #{time}"
    Character.update(ch.state, delta)
    
    last = curr
    sleep ts
}