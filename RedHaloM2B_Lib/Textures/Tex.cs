using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedHaloM2B.Textures
{
    public class TreeNode<T>
    {
        public string Name { get; set; }
        // 节点的值
        public T Value { get; set; }

        // 子节点
        public List<TreeNode<T>> Children { get; private set; }

        // 构造函数
        public TreeNode(T value)
        {
            Value = value;
            Children = new List<TreeNode<T>>();
        }

        // 添加子节点
        public void AddChild(TreeNode<T> child)
        {
            Children.Add(child);
        }

        // 遍历树 (深度优先)
        public void Traverse(Action<T> action)
        {
            action(Value); // 处理当前节点
            foreach (var child in Children)
            {
                child.Traverse(action); // 递归处理子节点
            }
        }
    }
}
/*
class Program
{
    static void Main(string[] args)
    {
        // 创建根节点
        var root = new TreeNode<string>("Root");

        // 添加子节点
        var child1 = new TreeNode<string>("Child 1");
        var child2 = new TreeNode<string>("Child 2");

        root.AddChild(child1);
        root.AddChild(child2);

        // 添加子节点的子节点
        child1.AddChild(new TreeNode<string>("Child 1.1"));
        child1.AddChild(new TreeNode<string>("Child 1.2"));

        child2.AddChild(new TreeNode<string>("Child 2.1"));

        // 遍历并打印树结构
        Console.WriteLine("Tree Traversal:");
        root.Traverse(value => Console.WriteLine(value));
    }
}
*/