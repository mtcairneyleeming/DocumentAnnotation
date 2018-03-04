import gzip
import json
from typing import List

from lxml import etree


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

    def analyse_element(self, el: etree.Element):
        if el.tag == "text" or (el.tag == "div1" and el.get("type").lower() == self.milestones[0]):
            if self.current_book.get("Name") != "" and len(self.processed_sections) > 0:
                # add previous book to list
                self.current_book["Sections"] = self.processed_sections
                self.processed_books.append(self.current_book)
            # start new book
            self.processed_sections = []
            self.current_book = {"Name": el.get("n"), "Sections": []}

        elif el.tag == "milestone":
            unit = el.get("unit")
            if unit == self.milestones[0]:  # book
                if self.current_book.get("Name") != "" and len(self.processed_sections) > 0:
                    # add previous book to list
                    self.current_book["Sections"] = self.processed_sections
                    self.processed_books.append(self.current_book)
                # start new book
                self.processed_sections = []
                self.current_book = {"Name": el.get("n"), "Sections": []}

            elif unit == self.milestones[1]:  # section
                if self.current_section.get("Name") != "" and len(self.processed_groups) > 0:
                    # add previous section to list
                    self.current_section["Groups"] = self.processed_groups
                    self.processed_sections.append(self.current_section)
                # start new section
                self.processed_groups = []
                self.current_section = {"Name": el.get("n"), "Groups": []}

        # as far as I can tell, the '3rd milestone', a group, never actually shows up in a citation hint, so it must
        # be artificially put together. I.e., for verse, split on <l> elements, for prose, split on elements.
        elif el.tag == "l":  # group, specifically for verse
            self.processed_groups.append(
                {"Name": el.get("n"), "Data": etree.tostring(el, method="text", encoding='UTF-8').decode('utf-8'),
                 "AddNewLine": self.use_new_line})
        else:  # some other kind of element, most likely a emphasis or person name
            if el.tag == "note" or el.tag == "hi":  # pass on these, as notes just repeat the text already seen
                self.add_text(el, False)
                return # skip children of notes
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

        self.output["Books"] = self.processed_books
        self.save_data()
