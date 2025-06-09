import os, sys
from configparser import ConfigParser
from Source.file_iterator import *
from Source.prune import NamePruner

CONFIG_FILE = "Config/Settings.ini"

def main(folder:str):
    ptree = path_tree(folder)

    parser = ConfigParser()
    if not os.path.exists(CONFIG_FILE):
        print(f"Configuration file '{CONFIG_FILE}' does not exist.")
        return
    parser.read(os.path.abspath(CONFIG_FILE))
    rules_file = parser.get('Paths', 'rules_file', fallback='Config/Rules.json')
    
    name_pruner = NamePruner(rules_file, 'textures')

    def prune_node(node:path_node) -> None:
        if node.isfile:
            try:
                new_name = name_pruner.execute(node.abspath)
                # print(f"Renamed '{node.abspath}' to '{new_name}'")
            except FileNotFoundError as e:
                print(e)

    ptree.node_task(ptree.root, prune_node)

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python main.py <asset_directory>")
        sys.exit(1)

    folder = handle_spaced_dir(sys.argv)
    if not os.path.exists(folder):
        print(f'Directory "{folder}" does not exist.')
        sys.exit(1)

    main(folder)