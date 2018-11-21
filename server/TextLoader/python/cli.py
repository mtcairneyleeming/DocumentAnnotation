# NB: this is done in python due to to the simplicity of transforming varying documents in python as compared to C#.
# Each document has a slightly different structure, which makes C# harder to work with. (I think...)
from typing import BinaryIO


import argparse
from transformer import Transformer
import glob

# read command line arguments



parser = argparse.ArgumentParser()

parser.add_argument("--input-directory", "-i", type=str, required=True, dest="input")
parser.add_argument("--recursive", "-r", default=False, required=True, dest="recursive")
parser.add_argument("--output-directory", "-o", type=str, required=True, dest="output")
parser.add_argument("--identifier", "-f", type=str, required=False)

args = parser.parse_args()

if args.identifier:
    tr = Transformer(args.input, args.output, args.identifier)
    tr.build_output()
else:
    # do all files in directory of form x.y.z.xml (not ...metadata.xml)
    files = None
    if args.recursive:
        files = glob.glob(args.input + "/**/*.xml", recursive=True)
    else:
        files = glob.glob(args.input + "/*.xml")
    for file in files:
        print(file)
        if file.endswith("metadata.xml"):
            pass
        else:
            tr = Transformer(args.input, args.output, file[:11])
            tr.build_output()