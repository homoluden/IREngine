#!ruby19
# encoding: utf-8
require "helpers"
require "logging"
IRE.log "Character Test: after requires" if $verbose
$: << File.expand_path(File.expand_path(File.dirname(__FILE__)))
IRE.log $:.to_s if $verbose

require_relative "game/character"

include System

ts = 0.01
ch = Character.build_discrete_model [1000.1, 1.5, 1.0], [1000.1, 1.5, 1.0], [0.01, 0.1], [0.01, 0.1], ts
IRE.log ch.print

ch.set_target_position 1.5, 3.0

#IRE.log "Trace 1"
time = 0.0
start = last = Environment.tick_count
50000.times{|i|
    curr = Environment.tick_count
    delta = curr - last
    time += delta / TimeSpan.ticks_per_second.to_f
    IRE.log "Time: #{time}\n#{ch.print}"
    Character.update(ch.state, delta)
    
    last = curr
    sleep ts
}