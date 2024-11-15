namespace InstantDeploy.Utils;

/// <summary>
/// 树基类
/// </summary>
public interface ITreeNode<TKey>
{
    /// <summary>
    /// 获取节点id
    /// </summary>
    /// <returns></returns>
    TKey GetId();

    /// <summary>
    /// 获取节点父id
    /// </summary>
    /// <returns></returns>
    TKey GetPId();

    /// <summary>
    /// 设置Children
    /// </summary>
    /// <param name="children"></param>
    void SetChildren(IList children);
}

/// <summary>
/// 递归工具类，用于遍历有父子关系的节点，例如菜单树，字典树等等
/// </summary>
/// <typeparam name="T"></typeparam>
public class TreeBuildUtils<TKey, T> where T : ITreeNode<TKey>, new()
{
    /// <summary>
    /// 顶级节点的父节点Id(默认Guid.Empty)
    /// </summary>
    private TKey _rootParentId = default!;

    /// <summary>
    /// 构造子节点集合
    /// </summary>
    /// <param name="totalNodes"></param>
    /// <param name="node"></param>
    /// <param name="childNodeList"></param>
    private void BuildChildNodes(List<T> totalNodes, T node, List<T> childNodeList)
    {
        var nodeSubList = new List<T>();
        totalNodes.ForEach(u =>
        {
            if (u.GetPId().Equals(node.GetId()))
                nodeSubList.Add(u);
        });
        nodeSubList.ForEach(u => BuildChildNodes(totalNodes, u, new List<T>()));
        childNodeList.AddRange(nodeSubList);
        node.SetChildren(childNodeList);
    }

    /// <summary>
    /// 设置根节点方法 查询数据可以设置其他节点为根节点，避免父节点永远是0，查询不到数据的问题
    /// </summary>
    public void SetRootParentId(TKey rootParentId) => _rootParentId = rootParentId;

    /// <summary>
    /// 构造树节点
    /// </summary>
    /// <param name="nodes"></param>
    /// <returns></returns>
    public List<T> Build(List<T> nodes)
    {
        nodes.ForEach(u => BuildChildNodes(nodes, u, new List<T>()));

        var result = new List<T>();
        nodes.ForEach(u =>
        {
            if (EqualityComparer<TKey>.Default.Equals(u.GetPId()))
                result.Add(u);
        });
        return result;
    }
}