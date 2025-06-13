import os, sys

def handle_spaced_dir(argv:list) -> str:
    res = ''
    n = len(argv)
    for i in range(1, n):
        res += argv[i]
        if i != n - 1:
            res += ' '
    return f'{res}'

class path_node:
    def __init__(self, path:str, prt=None) -> None:
        self.abspath = path
        self.relpath = os.path.basename(path)
        self.parent = prt
        self.isfile = os.path.isfile(self.abspath)
        self.children = []

    def add_child(self, child) -> None:
        child.parent = self
        self.children.append(child)

    def emplace_child(self, child_path:str):
        ch = path_node(child_path)
        self.add_child(ch)
        return ch
    
    def udpate_path(self):
        if not self.parent is None:
            self.abspath = os.path.join(self.parent.abspath, self.relpath)
            print(self.abspath)

    def rename(self, newname):
        self.udpate_path()

        basename, extension = os.path.splitext(self.abspath)

        extension = self.relpath.replace(extension, '')
        
        new_abspath = os.path.join(
            os.path.dirname(self.abspath),
            self.relpath.replace(extension, newname))

        os.rename(
            self.abspath,
            new_abspath)
        
        self.abspath = new_abspath
        self.relpath = self.relpath.replace(extension, newname)

class path_tree:
    def __init__(self, root_path:str, is_virtual:bool=False) -> None:
        self.root = path_node(root_path)

        if not self.root.isfile:
            self._recur_iter(self.root)

        self.is_virtual = is_virtual

    def _recur_iter(self, node:path_node) -> None:
        _dir = node.abspath

        curdir = os.curdir

        li = os.listdir(_dir)
        for p in li:
            _path = os.path.join(_dir, p)
            _node = node.emplace_child(_path)
            if os.path.isdir(_path):
               self._recur_iter(_node)

    def node_task(self, node:path_node, func) -> None:
        '''
        <b>Recursively</b> do specific task through tree or subtree.

            node: from which node to recur. self.root is recommended.
        
            func: task itself.
        '''

        # first recur then task
        for ch in [ch for ch in node.children if not ch.isfile]:
            self.node_task(ch, func)

        for ch in [ch for ch in node.children if ch.isfile]:
            func(ch)

        '''
        for ch in node.children:
            if ch.isfile:
                func(ch)
            else:
                self.node_task(ch, func)
        '''
        func(node)

    def copy_virtual(self, target_root:str):
        virtual_tree = path_tree(target_root)

        return virtual_tree

if __name__ == '__main__':
    if len(sys.argv) < 2:
        print('Empty Input')
        quit()
    
    work_dir = handle_spaced_dir(sys.argv)
    
    ptree = path_tree(work_dir)

    def print_node(node) -> None:
        print(node.abspath, '|' * 5, node.relpath)

    ptree.node_task(ptree.root, print_node)