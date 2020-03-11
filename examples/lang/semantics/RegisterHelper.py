registerCounter = 0

def GetNextRegister():
    global registerCounter
    register = "%" + str(registerCounter)
    registerCounter += 1
    return register

def ResetRegisters():
    global registerCounter
    registerCounter = 0