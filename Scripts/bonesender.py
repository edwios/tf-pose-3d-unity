#
#   Bone Sender in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"--start--" from client, replies with joints of skeleton, then b'--eof--'
#
#   Todos:
#   Use PUB-SUB model instead of REQ-REP
#

import time
import zmq
import argparse
import os

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

def main():
    parser = argparse.ArgumentParser(description='tf-pose-estimation run by folder')
    parser.add_argument('--folder', type=str, default='../datas/')
    parser.add_argument('--data', type=str, default='3d_data', help='Prefix of data file, default 3d_data#.txt')
    args = parser.parse_args()

    nf=0
    while True:
        message = socket.recv()
        print("Received request: %s" % message)
        if (message == b'--start--'):
            print("Starting")
            try:
                datafile = os.path.join(args.folder, args.data + str(nf) + ".txt")
            except:
                print("End of data supply")
                break
            print("Debug: reading from " + datafile)
            with open(datafile, 'r') as f:
                for l in f.readlines():
                    socket.send(bytes(l, encoding='utf-8'))
                    message = socket.recv()
                    if (message != b'--cont--'):
                        print("Unity closing " + datafile)
                        f.close()
                        break
                print("File ended. Closing " + datafile)
                f.close()
                socket.send(b'--eof--')
                nf = nf + 1
    print("Session ended")
    socket.send(b"--end--")

if __name__ == '__main__':
    main()
