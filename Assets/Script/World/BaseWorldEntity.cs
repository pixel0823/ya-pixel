using UnityEngine;
using Photon.Pun;
using YAPixel;

namespace YAPixel.World
{
    /// <summary>
    /// 월드에 존재하는 모든 상호작용 가능한 개체(아이템, 오브젝트 등)의 기본 클래스입니다.
    /// Photon 초기화 및 데이터 로딩과 같은 공통 로직을 처리합니다.
    /// </summary>
    /// <typeparam name="TData">이 개체의 데이터를 정의하는 ScriptableObject (예: Item, Object)</typeparam>
    /// <typeparam name="TDatabase">데이터를 관리하는 데이터베이스 (예: ItemDatabase, ObjectDatabase)</typeparam>
    public abstract class BaseWorldEntity<TData, TDatabase> : MonoBehaviourPun, IPunInstantiateMagicCallback
        where TData : ScriptableObject, IDatabaseItem
        where TDatabase : BaseDatabase<TData>
    {
        [Header("Base Entity Data")]
        [Tooltip("이 개체의 데이터. ScriptableObject를 참조합니다.")]
        public TData entityData;

        protected TDatabase database;

        /// <summary>
        /// 이 개체의 데이터베이스가 Resources 폴더 내에 있는 경로입니다.
        /// </summary>
        protected abstract string DatabasePath { get; }

        #region IPunInstantiateMagicCallback

        /// <summary>
        /// PhotonNetwork.Instantiate를 통해 생성될 때 호출됩니다.
        /// 네트워크로부터 데이터를 받아 개체를 초기화합니다.
        /// </summary>
        public virtual void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            // 1. 데이터베이스 로드
            database = Resources.Load<TDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError($"'{DatabasePath}' 경로에서 데이터베이스를 찾을 수 없습니다.");
                if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject);
                return;
            }

            // 2. 네트워크로부터 데이터 인덱스 추출
            object[] instantiationData = info.photonView.InstantiationData;
            if (instantiationData == null || instantiationData.Length == 0)
            {
                Debug.LogError("Instantiation data가 비어있습니다. 개체를 초기화할 수 없습니다.");
                if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject);
                return;
            }

            int dataIndex = System.Convert.ToInt32(instantiationData[0]);

            // 3. 데이터베이스에서 데이터 검색
            TData data = database.GetItem(dataIndex);
            if (data == null)
            {
                Debug.LogError($"데이터베이스에서 인덱스 {dataIndex}에 해당하는 데이터를 찾을 수 없습니다.");
                if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject);
                return;
            }

            // 4. 개체 초기화
            Initialize(data, instantiationData);
        }

        #endregion

        /// <summary>
        /// 검색된 데이터와 전체 instantiationData를 사용하여 개체를 초기화합니다.
        /// 하위 클래스에서 이 메서드를 구현하여 특정 초기화 로직을 수행해야 합니다.
        /// </summary>
        /// <param name="data">데이터베이스에서 찾은 개체 데이터</param>
        /// <param name="instantiationData">Photon에서 전달된 전체 데이터 배열</param>
        public abstract void Initialize(TData data, object[] instantiationData);
    }
}
