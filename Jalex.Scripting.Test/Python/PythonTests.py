class SimpleTest(object):
	def __init__(self, *args):
		self.Arg0 = 'Hi'
		self.Arg1, self.Arg2 = args

def TestMethod(*args):
	return "got " + args[0]
