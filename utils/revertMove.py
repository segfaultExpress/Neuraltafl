import sys

move = sys.argv[1]
print(move)
move = int(move)

div1 = move // 1331
rem1 = move % 1331
div2 = rem1 // 121
rem2 = rem1 % 121
div3 = rem2 // 11
rem3 = rem2 % 11
div4 = rem3


print("X1: {}".format(div4))
print("Y1: {}".format(div3))
print("")
print("X2: {}".format(div2))
print("Y2: {}".format(div1))

