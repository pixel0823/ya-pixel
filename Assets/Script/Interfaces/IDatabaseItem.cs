
namespace YAPixel
{
    /// <summary>
    /// 데이터베이스에 저장되는 항목이 공통적으로 가져야 하는 속성을 정의합니다.
    /// </summary>
    public interface IDatabaseItem
    {
        /// <summary>
        /// 데이터베이스에서 항목을 식별하는 데 사용되는 고유한 이름입니다.
        /// </summary>
        string Name { get; }
    }
}
