import gzip
import json
from typing import List

from lxml import etree


def int_try_parse(value):
    try:
        return int(value), True
    except ValueError:
        return value, False


# noinspection PyArgumentList
class Transformer:
    """Transforms TEI.2 xml documents from Perseus to a custom JSON format for simpler use with the C# server"""

    def __init__(self, input_dir, output_dir, identifier, use_new_line):
        # where to load and save data
        self.input_dir = input_dir
        self.output_dir = output_dir
        # identifier of document to save & load
        self.identifier = identifier

        # whether to use a new line between groups
        self.use_new_line = use_new_line
        # the final data to save
        self.output = {}

        # used while transformation is in progress, to keep track of state
        self.milestones = []

        self.processed_books = []
        self.current_book = {}
        self.processed_sections = []
        self.current_section = {}
        self.processed_groups = []
        self.current_text = ""

    def load_xml(self, filename: str) -> etree.Element:
        # get raw xml
        with open('%s/%s.xml' % (self.input_dir, filename), "rb") as input_file:
            parser = etree.XMLParser(remove_blank_text=True)
            tree = etree.parse(input_file, parser)
            return tree.getroot()

    def save_data(self):
        """Save the data in self.processed_books to the appropriate file"""

        # n.b. compressed to reduce disk usage
        with gzip.open('%s/%s.json.gz' % (self.output_dir, self.identifier), "wt", encoding="utf-8") as outfile:
            json.dump(self.output, outfile, ensure_ascii=False)
            print("All done!")

    def get_hierarchy(self):
        metadata = self.load_xml(self.identifier + ".metadata")

        # n.b. xpath is carefully specified to avoid issues where there is metadata for several documents in one file.
        hierarchies = metadata.xpath(('/metadata/document[@id="Perseus:text:{0}"]/datum[@key="perseus:Citation"]/text()'
                                      .format(self.identifier)))
        translator = str.maketrans('', '', "*")
        hierarchies: List[str] = [x.translate(translator) for x in hierarchies]
        # return first - hopefully the best way to do this automatically? TODO: find a better solution!
        self.milestones = hierarchies[0].split(":")

    def add_text(self, el, use_text=True):
        if el.text or el.tail:
            text = ""
            if el.text and use_text:
                text += el.text
            if el.tail:
                text += el.tail
            text = " ".join(text.split())
            self.processed_groups.append({"Data": text, "AddNewLine": self.use_new_line})

    def fix_names(self):
        """Go through the books and sections, and if when parsed as integers, the names of sections do not
        consecutively increase, set the names to the range """

        for book in self.processed_books:

            checks = []
            for x in book["Sections"]:
                print(x["Name"])
                checks.append(int_try_parse(x["Name"]))
            if all(check[1] for check in checks):
                # all sections are integers
                print("Yay!")

                int_names = [check[0] for check in checks]
                discontinuous_numbering = False
                for i in range(len(int_names)):
                    if i != len(int_names) - 1:

                        if int_names[i] +1  != int_names[i+1]:
                            # discontinuous naming
                            book["Sections"][i]["Name"] = "{0}-{1}".format(int_names[i], int_names[i +1] - 1)
                            discontinuous_numbering = True
                    else:
                        if discontinuous_numbering:
                            # only add "-end" if the sections previous were not chapters like Pro Milone, but lines
                            # like the Aeneid
                            book["Sections"][i]["Name"] = "{0}-end".format(int_names[i])

    def save_book(self, new_name):
        if self.current_book.get("Name") != "" and len(self.processed_sections) > 0:
            # add previous book to list
            self.current_book["Sections"] = self.processed_sections
            self.processed_books.append(self.current_book)
        # start new book
        self.processed_sections = []
        self.current_book = {"Name": new_name, "Sections": []}


    def save_section(self, new_name):
        if self.current_section.get("Name") != "" and len(self.processed_groups) > 0:
            # add previous section to list
            self.current_section["Groups"] = self.processed_groups
            self.processed_sections.append(self.current_section)
        # start new section
        self.processed_groups = []
        self.current_section = {"Name": new_name, "Groups": []}


    def analyse_element(self, el: etree.Element):
        if el.tag == "text" or (el.tag == "div1" and el.get("type").lower() == self.milestones[0]):
            self.save_section("")
            self.save_book(el.get("n"))

        elif el.tag == "milestone":
            unit = el.get("unit")
            if unit == self.milestones[0]:  # book
                self.save_section("")
                self.save_book(el.get("n"))

            elif unit == self.milestones[1]:  # section
                self.save_section(el.get("n"))

        # as far as I can tell, the '3rd milestone', a group, never actually shows up in a citation hint, so it must
        # be artificially put together. I.e., for verse, split on <l> elements, for prose, split on elements.
        elif el.tag == "l":  # group, specifically for verse
            self.processed_groups.append(
                {"Name": el.get("n"), "Data": etree.tostring(el, method="text", encoding='UTF-8').decode('utf-8'),
                 "AddNewLine": self.use_new_line})
        else:  # some other kind of element, most likely a emphasis or person name
            if el.tag == "head":  # for the head, add a pointless title
                return;
            if el.tag == "note" or el.tag == "hi" :  # pass on these, as notes just repeat the
                # text already seen
                self.add_text(el, False)
                return  # skip children of notes
            if el.text or el.tail:
                self.add_text(el, True)

        # now run this again for any child elements, so that the correct order is preserved.

        for child in el:
            self.analyse_element(child)

        return

    def build_output(self):
        # load xml file
        root = self.load_xml(self.identifier)

        # text name
        names = root.xpath('.//titleStmt/title[not(@type)]')
        self.output["Name"] = ";".join([n.text for n in names])

        text_root: etree.Element = root.xpath("/TEI.2/text")[0]
        self.get_hierarchy()
        self.analyse_element(text_root)

        # tidy up loose ends - the last book/etc has to be added, as there will still be data to add, as there is no
        # final element
        self.save_section("Finished sections")
        self.save_book("Finished books")


        self.fix_names()

        self.output["Books"] = self.processed_books
        self.save_data()
