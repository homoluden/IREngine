#!ruby19
# encoding: utf-8

include IREngine
if self.logger
    $logger = self.logger
    $logger.write_message "Got logger from ScriptScope" if $verbose
else
    $logger = IRE.instance
    puts "Created new logger"
    $logger.write_message "Created new logger" if $verbose
end
 
class IRE
    def IRE.log(message)
        $logger.write_message message
        puts message
    end
    
    def IRE.err(message)
        $logger.write_error message
    end
end

IRE.log "Logger test" if $verbose
