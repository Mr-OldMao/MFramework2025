//using GameMain.Generate.FlatBuffers;
//using UnityEngine;

//public class TestParseData : MonoBehaviour
//{
//    // Start is called before the first frame update
//    void Start()
//    {
//        LoadBytesFromResources();
//    }

//    public string fileName = "fb/TestTemplate_Tab1";

//    public void LoadBytesFromResources()
//    {
//        try
//        {
//            TextAsset textAsset = Resources.Load<TextAsset>(fileName);

//            if (textAsset != null)
//            {
//                // 获取字节数据
//                byte[] data = textAsset.bytes;

//                Debug.Log($"通过Resources加载成功: {fileName}");
//                Debug.Log($"文件大小: {data.Length} 字节");

//                // 这里可以开始解析FlatBuffer数据
//                ParseFlatBufferData(data);

//            }
//            else
//            {
//                Debug.LogError($"Resources中未找到文件: {fileName}");
//            }
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError($"加载文件失败: {e.Message}");
//        }
//    }

//    void ParseFlatBufferData(byte[] data)
//    {
//        try
//        {
//            var buffer = new Google.FlatBuffers.ByteBuffer(data);

//            //有报错：编辑器工具一键生成，生成Excel模板文件 =》 打表 =》 一键拷贝.bytes到Resources文件夹    
//            var tableArray = FB_TestTemplate_Tab1_Array.GetRootAsFB_TestTemplate_Tab1_Array(buffer);

//            for (int i = 0; i < tableArray.DatasLength; i++)
//            {
//                var table = tableArray.Datas(i).Value;

//                var fieldNames = table.GetType().GetFields();

//                foreach (var item in fieldNames)
//                {
//                    Debug.Log(item.Name + " " + item.GetValue(table));
//                }

//                Debug.Log("------------------------------------------------------");

//                var props = table.GetType().GetProperties();

//                foreach (var item in props)
//                {
//                    Debug.Log("key:" + item.Name + "  ,  Value:" + item.GetValue(table));
//                }
//            }
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError($"解析失败: {e.Message}");
//        }
//    }
//}
