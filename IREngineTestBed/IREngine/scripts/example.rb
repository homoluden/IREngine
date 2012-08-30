#!ruby19
# encoding: utf-8

include IREngine

class IRE
    def log(message)
        IRE.Instance.write_message message
    end
    
    def err(message)
        IRE.Instance.write_error message
    end
end

def log(message, is_err = false)
    IRE.Instance.log message unless is_err
    IRE.Instance.err message if is_err
end

5.times{|i|
    log "Hello, Output! (from Ruby Async Task)"
    log "Hello, Error! (from Ruby Async Task)", true
}

raise "\n!!!\tBOOO! Catch super scaring exception from RUBY...\t!!!\n"