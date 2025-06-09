import os, json

class NamePruner:
    def __init__(self, rule_f:str, rule_type:str='textures') -> None:
        self._rule_file = rule_f
        self.rule_type = rule_type
        self._load_rules()
    
    def _load_rules(self):
        if not os.path.exists(self._rule_file):
            raise FileNotFoundError(f"Rule file '{self._rule_file}' does not exist.")
        
        with open(self._rule_file, 'r', encoding='utf-8') as f:
            rules = json.load(f)
        f.close()

        self.rules = rules.get(self.rule_type, {})
        self._ood_prefix_list = self.rules.get('ood_prefix', [])
        self._ood_suffix_list = self.rules.get('ood_suffix', [])
        self._new_prefix = self.rules.get('new_prefix', '')
        self._new_suffix = self.rules.get('new_suffix', '')

        self._ood_prefix_list.sort(key=len, reverse=True)
        self._ood_suffix_list.sort(key=len, reverse=True)

    def execute(self, fpath:str) -> str:
        if not os.path.exists(fpath):
            raise FileNotFoundError(f"Path '{fpath}' does not exist.")
        
        basename, extension = os.path.splitext(os.path.basename(fpath))
        
        has_prefix_matched = False
        has_suffix_matched = False

        for prefix in self._ood_prefix_list:
            if prefix == '':
                continue

            if basename.startswith(prefix):
                basename = basename[len(prefix):]
                print(f"Removed prefix '{prefix}' from '{fpath}'")
                has_prefix_matched = True
                break

        for suffix in self._ood_suffix_list:
            if suffix == '':
                continue

            if basename.endswith(suffix):
                basename = basename[:-len(suffix)]
                print(f"Removed suffix '{suffix}' from '{fpath}'")
                has_suffix_matched = True
                break

        if not has_prefix_matched and not has_suffix_matched:
            return fpath
        
        # rename with abspath
        new_basename = f"{self._new_prefix}{basename}{self._new_suffix}{extension}"
        new_fpath = os.path.join(os.path.dirname(fpath), new_basename)

        print(f"Renaming '{fpath}' to '{new_fpath}'")

        os.rename(fpath, new_fpath)

        return new_fpath