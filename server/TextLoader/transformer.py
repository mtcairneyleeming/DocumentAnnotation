# NB: this is done in python due to to the simplicity of transforming varying documents in python as compared to C#. Each document has a slightly different structure, which makes C# harder to work with. (I think...)

from lxml import etree
import argparse
import json
import gzip
# read command line arguments

parser = argparse.ArgumentParser()

parser.add_argument("--input", "-i", type=str, required=True)
parser.add_argument("--output", "-o", type=str, required=True)
parser.add_argument("--hierarchy", "-r", type=str, required=True)
parser.add_argument("--add-new-line", "-n", type=bool, required=True, dest="newLine")
parser.add_argument("--db-id", "-d", type=int, required=True, dest="id")

args = parser.parse_args()

# load xml file

with open(args.input, "r") as input_file:
    parser = etree.XMLParser(remove_blank_text=True)
    tree = etree.parse(input_file, parser)

    root = tree.getroot()
    output = dict()

    ## begin parsing appropriate data

    # text id
    output["DBId"] = args.id
    # text name
    names = root.xpath('.//titleStmt/title[not(@type)]')
    output["Name"] = ";".join([n.text for n in names])

    ### books
    # there are a lot of different ways texts are organised, for the moment these are hardcoded
    # as cli inputs that map to the appropriate selection
    processed_books = []
    if args.hierarchy == "book:card:line": # e.g. Aeneid
        books = root.xpath("/TEI.2/text/body/*")
        for book in books:
            current_book = {"Name": book.get("n"), "Sections": []}
            milestone_lines = []
            milestone_name = ""
            for child in book:
                if child.tag == "milestone":
                    if len(milestone_lines) > 0:
                        # last set needs to be added to the section
                        current_book["Sections"].append({"Name": milestone_name, "Groups": milestone_lines})
                    # hopefully this is the first milestone then
                    milestone_name = child.get("n")
                else:
                    # hopefully a line of text
                    milestone_lines.append({"Name": child.get("n"), "Data": child.text, "AddNewLine": args.newLine})
            processed_books.append(current_book)
            # print (book.tag)
    elif args.hierarchy == "text:chapter:section": # e.g. Aeneid
        books = root.xpath("/TEI.2/text/body/*")
        for book in books:
            current_book = {"Name": book.get("n"), "Sections": []}
            milestone_lines = []
            milestone_name = ""
            for child in book:
                if child.tag == "milestone":
                    if len(milestone_lines) > 0:
                        # last set needs to be added to the section
                        current_book["Sections"].append({"Name": milestone_name, "Groups": milestone_lines})
                    # hopefully this is the first milestone then
                    milestone_name = child.get("n")
                else:
                    # hopefully a line of text
                    milestone_lines.append({"Name": child.get("n"), "Data": child.text, "AddNewLine": args.newLine})
            processed_books.append(current_book)
            # print (book.tag)
    else:
        print( "Unrecognised hierarchy format")
    output["Books"] = processed_books
    with gzip.open(args.output, "wb") as outfile: # n.b. compressed to reduce disk usage
        json.dump(output, outfile)
        print ("All done!")


