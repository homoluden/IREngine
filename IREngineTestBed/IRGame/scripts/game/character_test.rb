﻿#!ruby19
# encoding: utf-8
require "helpers"
require "logging"
IRE.log "Character Test: after requires" if $verbose
$: << File.expand_path(File.expand_path(File.dirname(__FILE__)))
IRE.log $:.to_s if $verbose

require_relative "game/character"

ts = 0.01
ch = Character.build_discrete_model [0.01, 0.5, 1.0], [0.01, 0.5, 1.0], [0.01, 0.1], [0.01, 0.1], ts
IRE.log ch.print

ch.set_target_position 0.5, 1.0

600.times{|i|
    Character.update(ch.state, ts)
    IRE.log "Time: #{i*ts}\n#{ch.print}"
    sleep ts
}