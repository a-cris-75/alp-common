using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using System.IO;
using log4net;
using System.Data;
using System.Net.Mail;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Specialized;
using log4net.Appender;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace Alp.Com.Igu.Utils
{
    public static class UtilsObj
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

       
        public static bool CopyObjectToObject_LOG_EN = false;

        /// <summary>
        /// Copia un oggetto strutturato (classe o struct) da un src a un dest.
        /// src e dest sono classi o struct dello stesso tipo.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static bool CopyObjectToObjectPF(Object src, Object dest)
        {
            bool res = false;
            try
            {
                // carica la lista delle proprietà della classe di destinazione
                PropertyInfo[] properties = dest.GetType().GetProperties();
                Hashtable hashtableDest = new Hashtable();
                FieldInfo[] fields = dest.GetType().GetFields();
                foreach (PropertyInfo info in properties)
                    hashtableDest[info.Name.ToUpper()] = info;
                // se il tipo non contiene PROPERTIES ma FIELDS
                foreach (FieldInfo info in fields)
                    hashtableDest[info.Name.ToUpper()] = info;

                Hashtable hashtableSource = new Hashtable();
                FieldInfo[] fieldsS = src.GetType().GetFields();
                PropertyInfo[] propS = src.GetType().GetProperties();
                foreach (PropertyInfo info in propS)
                    hashtableSource[info.Name.ToUpper()] = info;
                // se il tipo non contiene PROPERTIES ma FIELDS
                foreach (FieldInfo info in fieldsS)
                    hashtableSource[info.Name.ToUpper()] = info;

                foreach (DictionaryEntry pi in hashtableSource)
                {
                    // se la proprietà esiste nella classe di destinazione
                    //PropertyInfo pid = properties.Where(x => x.Name.Equals(pi.Name)).FirstOrDefault<PropertyInfo>();
                    PropertyInfo? infoPDest = null;
                    FieldInfo? infoFDest = null;

                    string pikeyup = ""; 
                    if(pi.Key != null)
                        pikeyup = ((string)pi.Key).ToUpper();


                    if (hashtableDest!=null && hashtableDest.ContainsKey(pikeyup))
                    {
                        if (hashtableDest[pikeyup] is PropertyInfo)
                            infoPDest = (PropertyInfo)hashtableDest[pikeyup];
                        else
                            infoFDest = (FieldInfo)hashtableDest[pikeyup];
                    }
                    try
                    {
                        if ((infoPDest != null) && infoPDest.CanWrite)
                        {
                            object? v = null;
                            if (hashtableSource[pikeyup] is PropertyInfo)
                                v = ((PropertyInfo)hashtableSource[pikeyup]).GetValue(src, null);//pi.Value;
                            if (hashtableSource[pikeyup] is FieldInfo)
                                v = ((FieldInfo)hashtableSource[pikeyup]).GetValue(src); //pi.Value;
                            Type t = Nullable.GetUnderlyingType(infoPDest.PropertyType) ?? infoPDest.PropertyType;

                            object? safeValue = null;
                            if (v is System.IConvertible)
                                safeValue = (v == null) ? null : Convert.ChangeType(v, t);

                            if (safeValue != null || infoPDest.GetType() == typeof(Nullable<>))
                                infoPDest.SetValue(dest, safeValue, null);
                        }
                        else
                        if ((infoFDest != null))
                        {
                            object v = null;
                            if (hashtableSource[pikeyup] is PropertyInfo)
                                v = ((PropertyInfo)hashtableSource[pikeyup]).GetValue(src, null);//pi.Value;
                            if (hashtableSource[pikeyup] is FieldInfo)
                                v = ((FieldInfo)hashtableSource[pikeyup]).GetValue(src);
                                                                                                          

                            Type t = Nullable.GetUnderlyingType(infoFDest.FieldType) ?? infoFDest.FieldType;

                            object safeValue = null;
                            if (v is System.IConvertible)
                                safeValue = (v == null) ? null : Convert.ChangeType(v, t);

                            if (safeValue != null || infoFDest.GetType() == typeof(Nullable<>))
                                infoFDest.SetValue(dest, safeValue, BindingFlags.Default, null, System.Globalization.CultureInfo.CurrentCulture);
                        }
                    }
                    catch (Exception ex) { }
                }
                res = true;
            }
            catch (Exception ex)
            {
                if (CopyObjectToObject_LOG_EN)
                {
                    log.Error("CopyObjectToObject: " + ex.Message);
                }
                throw ex;
            }

            return res;
        }

        public static bool CopyDictionaryToObject(Dictionary<string, object> src, Object dest)
        {
            bool res = false;
            try
            {
                // carica la lista delle proprietà della classe di destinazione
                PropertyInfo[] properties = dest.GetType().GetProperties();
                Hashtable hashtableDest = new Hashtable();
                FieldInfo[] fields = dest.GetType().GetFields();
                foreach (PropertyInfo info in properties)
                    hashtableDest[info.Name.ToUpper()] = info;
                // se il tipo non contiene PROPERTIES ma FIELDS
                foreach (FieldInfo info in fields)
                    hashtableDest[info.Name.ToUpper()] = info;

                foreach (KeyValuePair<string, object> kv in src)
                {
                    object v = kv.Value;

                    if (v != null) 
                    {
                        // se la proprietà esiste nella classe di destinazione
                        //PropertyInfo pid = properties.Where(x => x.Name.Equals(pi.Name)).FirstOrDefault<PropertyInfo>();
                        PropertyInfo? infoPDest = null;
                        FieldInfo? infoFDest = null;
                        if (hashtableDest.ContainsKey(kv.Key.ToUpper())) 
                        {
                            if( hashtableDest[kv.Key.ToUpper()] is PropertyInfo)
                                infoPDest = (PropertyInfo)hashtableDest[kv.Key.ToUpper()];
                            else
                                infoFDest = (FieldInfo)hashtableDest[kv.Key.ToUpper()];
                        }
                        try
                        {
                            if ((infoPDest != null) && infoPDest.CanWrite)
                            {
                                Type t = Nullable.GetUnderlyingType(infoPDest.PropertyType) ?? infoPDest.PropertyType;

                                object? safeValue = null;
                                if (v is System.IConvertible)
                                    safeValue = (v == null) ? null : Convert.ChangeType(v, t);

                                if (safeValue != null || infoPDest.GetType() == typeof(Nullable<>))
                                    infoPDest.SetValue(dest, safeValue, null);
                            }
                            else
                            if ((infoFDest != null))
                            {
                                Type t = Nullable.GetUnderlyingType(infoFDest.FieldType) ?? infoFDest.FieldType;

                                object? safeValue = null;
                                if (v is System.IConvertible)
                                    safeValue = (v == null) ? null : Convert.ChangeType(v, t);

                                if (safeValue != null || infoFDest.GetType() == typeof(Nullable<>))
                                    infoFDest.SetValue(dest, safeValue, BindingFlags.Default, null, System.Globalization.CultureInfo.CurrentCulture);
                            }
                        }
                        catch (Exception ex) { }
                    }                   
                }
                res = true;
            }
            catch (Exception ex)
            {
                if (CopyObjectToObject_LOG_EN)
                {
                    log.Error("CopyObjectToObject: " + ex.Message);
                }
                throw ex;
            }

            return res;
        }


        /// <summary>
        /// Copia un buffer (string) in un oggetto deserializzandolo in base al tipo di dati dell'oggetto destinatario.
        /// Se partname è valorizzato considera solo i campi contenenti la stringa partname
        /// E' utile se ho una struttura, contenente un header e un insieme di campi da riempire con determinati valori (per es recuperati dal L1)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static bool CopyArrayToObjectByPosition(object[] values, Object dest, string? partname )
        {
            bool res = true;
            try
            {
                // carica la lista delle proprietà della classe di destinazione
                Type T = dest.GetType();
                PropertyInfo[] properties = T.GetProperties();
                Hashtable hashtableDest = new Hashtable();
                FieldInfo[] fields = dest.GetType().GetFields();
                foreach (PropertyInfo info in properties)
                    hashtableDest[info.Name.ToUpper()] = info;
                // se il tipo non contiene PROPERTIES ma FIELDS
                foreach (FieldInfo info in fields)
                    hashtableDest[info.Name.ToUpper()] = info;

                MemberInfo[]? members = null;
                MemberInfo[]? members2 = null;
                if (!string.IsNullOrEmpty(partname))
                {
                    // identifico solo i campi con un determinato prefisso: in questo modo copio i valori in quei campi
                    // è utile se ho una struttura, contenente un header e un insieme di campi da riempire con determinati valori (per es recuperati dal L1)

                    members = T.GetFields().Cast<MemberInfo>()
                                        .Where(X => X.Name.ToUpper().Contains(partname)).ToArray();
                    members2 = T.GetProperties().Where(X => X.Name.ToUpper().ToUpper().Contains(partname)).ToArray();
                }
                else
                {
                    members = T.GetFields().Cast<MemberInfo>().ToArray();
                    members2 = T.GetProperties().ToArray();
                }

                if(members!=null && members.Length>0 && members2!=null) 
                    members.Concat(members2);
                else 
                if(members2!=null && members2.Length>0)
                    members = members2;

                
                //solo se il numero di campi è maggiore uguale al numero di valori
                if (members!=null && values!=null && members.Count() >= values.Count())
                {
                    //for (int i = 0; i < members.Length; i++)
                    for (int i = 0; i < values.Count(); i++)
                    {
                        MemberInfo m = members[i];
                        if (m.MemberType == MemberTypes.Property)
                        {
                            PropertyInfo infoPDest = (PropertyInfo)hashtableDest[m.Name];
                            Type t = Nullable.GetUnderlyingType(infoPDest.PropertyType) ?? infoPDest.PropertyType;
                            object safeValue = null;
                            if (values[i] is System.IConvertible)
                                safeValue = (values[i] == null) ? null : Convert.ChangeType(values[i], t);

                            if (safeValue != null || infoPDest.GetType() == typeof(Nullable<>))
                                infoPDest.SetValue(dest, safeValue, null);
                        }
                        else
                        if (m.MemberType == MemberTypes.Field)
                        {
                            FieldInfo infoPDest = (FieldInfo)hashtableDest[m.Name];
                            Type t = Nullable.GetUnderlyingType(infoPDest.FieldType) ?? infoPDest.FieldType;
                            object safeValue = null;
                            if (values[i] is System.IConvertible)
                                safeValue = (values[i] == null) ? null : Convert.ChangeType(values[i], t);

                            if (safeValue != null || infoPDest.GetType() == typeof(Nullable<>))
                                infoPDest.SetValue(dest, safeValue, BindingFlags.Default, null, System.Globalization.CultureInfo.CurrentCulture);
                        }

                    }
                    res = true;
                }
               
            }
            catch (Exception ex)
            {
                res = false;
                if (CopyObjectToObject_LOG_EN)
                {
                    log.Error("CopyArrayToObjectByPosition: " + ex.Message);
                }
                throw ex;
            }

            return res;
        }

        public static List<MemberInfo>? GetMemberInfo(System.Type T)
        {
            List<MemberInfo> members_all = T.GetFields().Cast<MemberInfo>().ToList();
            MemberInfo[] members_allP = T.GetProperties().ToArray();
            if (members_all != null && members_allP != null && members_allP.Count() > 0)
                members_all.AddRange(members_allP.ToList());
            else
            if (members_allP != null && members_allP.Count() > 0)
                members_all = members_allP.ToList();

            return members_all;
        }

        public static List<(string,Type)> GetNameTypeMemberInfo(System.Type T)
        {
            List<(string, Type)> res = new List<(string, Type)>();
            List<MemberInfo> members_all = T.GetFields().Cast<MemberInfo>().ToList();
            MemberInfo[] members_allP = T.GetProperties().ToArray();
            if (members_all != null && members_allP != null && members_allP.Count() > 0)
                members_all.AddRange(members_allP.ToList());
            else
            if (members_allP != null && members_allP.Count() > 0)
                members_all = members_allP.ToList();

            if (members_all != null)
            {
                Type td = "string".GetType();
                foreach (MemberInfo m in members_all)
                {
                    (string, Type) itm = (m.Name, td);
                    if (m is PropertyInfo)
                        itm.Item2 = ((PropertyInfo)m).PropertyType;
                    if (m is FieldInfo)
                        itm.Item2 = ((FieldInfo)m).FieldType;
                    
                    res.Add(itm);
                }
            }
            return res;
        }

        /// <summary>
        /// Restiruisce lista di Property/Field name, prop/Field type, value
        /// </summary>
        /// <param name="OBJ"></param>
        /// <returns></returns>
        public static List<(string, Type, object?)> GetNameTypeValMemberInfo(object OBJ)
        {
            System.Type T = OBJ.GetType();
            List<(string, Type, object?)> res = new List<(string, Type, object?)>();
            List<MemberInfo> members_all = T.GetFields().Cast<MemberInfo>().ToList();
            MemberInfo[] members_allP = T.GetProperties().ToArray();
            if (members_all != null && members_allP != null && members_allP.Count() > 0)
                members_all.AddRange(members_allP.ToList());
            else
            if (members_allP != null && members_allP.Count() > 0)
                members_all = members_allP.ToList();

            if (members_all != null)
            {
                Type td = "string".GetType();
                foreach (MemberInfo m in members_all)
                {
                    (string, Type, object?) itm = (m.Name, td, null);
                    if (m is PropertyInfo)
                    {
                        itm.Item2 = ((PropertyInfo)m).PropertyType;
                        itm.Item3 = ((PropertyInfo)m).GetValue(OBJ);
                    }
                    if (m is FieldInfo)
                    {
                        itm.Item2 = ((FieldInfo)m).FieldType;
                        itm.Item3 = ((FieldInfo)m).GetValue(OBJ);
                    }
                    res.Add(itm);
                }
            }
            return res;
        }

        public static Type? GetTypePropertyFieldObj(Type tobjct, string propertyfield_name)
        {
            List<MemberInfo>? lst = UtilsObj.GetMemberInfo(tobjct);
            Type? res1 = null;
            if (lst != null)
            {
                MemberInfo? tag = lst.Where(X => X.Name.Equals(propertyfield_name)).FirstOrDefault();
                if (tag != null)
                {
                    if (tag is PropertyInfo)
                        res1 = ((PropertyInfo)tag).PropertyType;
                    if (tag is FieldInfo)
                        res1 = ((FieldInfo)tag).FieldType;
                }
            }
            return res1;
        }

        public static bool CopyObjectToObject_old(Object src, Object dest)
        {
            bool res = false;
            try
            {
                // carica la lista delle proprietà della classe di destinazione
                PropertyInfo[] properties = dest.GetType().GetProperties();

                // per ogni proprietà della classe da copiare
                foreach (PropertyInfo pi in src.GetType().GetProperties())
                {
                    // se la proprietà esiste nella classe di destinazione
                    PropertyInfo pid = properties.Where(x => x.Name.Equals(pi.Name)).FirstOrDefault<PropertyInfo>();

                    if (pid != null && pid.CanWrite)
                    {
                        try
                        {
                            // verifica se è una proprietà indicizzata
                            ParameterInfo[] src_params = pi.GetIndexParameters();
                            //ParameterInfo[] dest_params = pid.GetIndexParameters();

                            // l'array dei parametri a zero quando non è indicizzata
                            if (src_params.Count() == 0)
                            {
                                // carica il valore su un variant
                                var val = pi.GetValue(src, null);

                                // risali al tipo dato sorgente e destinazione
                                Type t = Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType;
                                Type td = Nullable.GetUnderlyingType(pid.PropertyType) ?? pid.PropertyType;

                                // notifica che il tipo destinazione è diverso dal tipo sorgente !!! ATTENZIONE CHE I LOG RALLENTANO MOLTO L'ESECUZIONE !!!
                                if (!td.FullName.Equals(t.FullName))
                                {
                                    if (CopyObjectToObject_LOG_EN)
                                    {
                                        log.Info("[CopyObjectToObject] PROPERTY [" + pi.Name + "] TIPO SRC [" + t.FullName + "] TIPO DEST [" + td.FullName + "]");
                                    }
                                }

                                // converti il variant nel tipo della proprietà di destinazione
                                object safeValue = null;
                                if (val is System.IConvertible)
                                {
                                    safeValue = (val == null) ? null : Convert.ChangeType(val, td); //safeValue = (val == null) ? null : Convert.ChangeType(val, t); <-- modifica in data 15/05/2016 
                                }

                                // assegna il valore
                                if (safeValue != null || pid.GetType() == typeof(Nullable<>))
                                    pid.SetValue(dest, safeValue, null);
                            }
                            else
                            {
                                foreach (ParameterInfo param in src_params)
                                {
                                    // carica il valore su un variant
                                    var val = pi.GetValue(src, new object[] { param });

                                    // risali al tipo dato sorgente e destinazione
                                    Type t = Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType;
                                    Type td = Nullable.GetUnderlyingType(pid.PropertyType) ?? pid.PropertyType;

                                    // notifica che il tipo destinazione è diverso dal tipo sorgente !!! ATTENZIONE CHE I LOG RALLENTANO MOLTO L'ESECUZIONE !!!
                                    if (!td.FullName.Equals(t.FullName))
                                    {
                                        if (CopyObjectToObject_LOG_EN)
                                        {
                                            log.Info("[CopyObjectToObject] PROPERTY [" + pi.Name + "] TIPO SRC [" + t.FullName + "] TIPO DEST [" + td.FullName + "]");
                                        }
                                    }

                                    // converti il variant nel tipo della proprietà di destinazione
                                    object safeValue = null;
                                    if (val is System.IConvertible/* && td is System.IConvertible*/)
                                    {
                                        safeValue = (val == null) ? null : Convert.ChangeType(val, td); //safeValue = (val == null) ? null : Convert.ChangeType(val, t); <-- modifica in data 15/05/2016 
                                    }

                                    // assegna il valore
                                    if (safeValue != null || pid.GetType() == typeof(Nullable<>))
                                        pid.SetValue(dest, safeValue, new object[] { param });
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            if (CopyObjectToObject_LOG_EN)
                            {
                                log.Error("[CopyObjectToObject] PROPERTY [" + pi.Name + "]" + err.Message);
                            }
                        }
                    }
                }
                res = true;
            }
            catch (Exception ex)
            {
                if (CopyObjectToObject_LOG_EN)
                {
                    log.Error("CopyObjectToObject: " + ex.Message);
                }
                throw ex;
            }

            return res;
        }

        public static object CopyObjectToObject(Object src, Type typeItemDest)
        {
            object dest = Activator.CreateInstance(typeItemDest);
            try
            {
                PropertyInfo[] pidlist = typeItemDest.GetProperties();
                foreach (PropertyInfo pi in src.GetType().GetProperties())
                {
                    PropertyInfo pid = pidlist
                            .Where(x => x.Name == pi.Name)
                            .FirstOrDefault<PropertyInfo>();

                    if (pid != null && pid.CanWrite)
                    {
                        try
                        {
                            var val = pi.GetValue(src, null);
                            Type t = Nullable.GetUnderlyingType(pi.PropertyType)
                                            ?? pi.PropertyType;

                            object safeValue = null;
                            if (val is System.IConvertible)
                                safeValue = (val == null) ? null : Convert.ChangeType(val, t);

                            if (safeValue != null || pid.GetType() == typeof(Nullable<>))
                                pid.SetValue(dest, safeValue, null);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("CopyObjectToObject: " + ex.Message);
                throw ex;
            }

            return dest;
        }

        public static bool CopyStructToObject(Object src, Object dest)
        {
            bool res = false;
            try
            {
                PropertyInfo[] pidlist = dest.GetType().GetProperties();
                foreach (FieldInfo pi in src.GetType().GetFields())
                {
                    PropertyInfo pid = pidlist
                            .Where(x => x.Name == pi.Name)
                            .FirstOrDefault<PropertyInfo>();

                    if (pid != null && pid.CanWrite)
                    {
                        try
                        {
                            var val = pi.GetValue(src);
                            Type t = Nullable.GetUnderlyingType(pi.FieldType)
                                            ?? pi.FieldType;
                            object safeValue = null;
                            if (val is System.IConvertible)
                                safeValue = (val == null) ? null : Convert.ChangeType(val, t);

                            if (safeValue != null || pid.GetType() == typeof(Nullable<>))
                                pid.SetValue(dest, safeValue, null);
                        }
                        catch { }
                    }
                }
                res = true;
            }
            catch (Exception ex)
            {
                log.Error("CopyStructToObject: " + ex.Message);
                throw ex;
            }

            return res;
        }

        public static object? GetValueFromObject(Object src, string propertyFieldName)
        {
            object? safeValue = null;
            try
            {
                // carica la lista delle proprietà della classe di destinazione
                if (src != null)
                {
                    PropertyInfo[] pidlist = src.GetType().GetProperties();
                    PropertyInfo? pid = pidlist.Where(x => x.Name.Equals(propertyFieldName)).FirstOrDefault<PropertyInfo>();
                    // converti il variant nel tipo della proprietà di destinazione
                    if (pid != null)
                        safeValue = pid.GetValue(src, null);
                    else
                    {
                        FieldInfo[] fieldsS = src.GetType().GetFields();
                        FieldInfo? fid = fieldsS.Where(x => x.Name.Equals(propertyFieldName)).FirstOrDefault<FieldInfo>();
                        // converti il variant nel tipo della proprietà di destinazione
                        if (fid != null)
                            safeValue = fid.GetValue(src);
                    }
                }
            }
            catch { }

            return safeValue;
        }

        public static List<T> CopyListToList<T>(List<object> src) where T : class
        {
            List<T> dest = new List<T>();
            foreach (object t in src)
            {
                object dc = CopyObjectToObject(t, typeof(T));
                dest.Add((T)dc);
            }
            return dest;
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T GetNewObject<T>() where T : new()
        {
            return new T();
        }

        public static bool IsNumber(string s)
        {
            bool res = true;
            if (string.IsNullOrEmpty(s))
                res = false;
            else
            {
                for (int i = 0; i < s.Length; i++)
                {
                    res = char.IsNumber(s, i);
                    if (!res)
                        res = (s[i] == ',' || s[i] == '.');
                    if (!res && i == 0)
                        res = (s[i] == '+' || s[i] == '-');
                    if (!res) return false;
                }
            }
            return res;
        }

        public static bool IsNumber(object s)
        {
            bool res = true;
            if (s == null)
                res = false;
            else
            {
                if (s is byte
                    || s is short
                    || s is ushort
                    || s is int
                    || s is uint
                    || s is long
                    || s is ulong
                    || s is float
                    || s is double
                    || s is decimal
                    ) res = true;
                else
                if (s is string)
                {
                    string ss = (string)s;
                    res = false;
                    for (int i = 0; i < ss.Length; i++)
                    {
                        res = char.IsNumber(ss, i);
                        if (!res)
                            res = (ss[i] == ',' || ss[i] == '.' || ss[i] == '-');
                        if (!res && i == 0)
                            res = (ss[i] == '+' || ss[i] == '-');
                        if (!res) return false;
                    }
                }
                else
                    res = false;
            }
            return res;
        }

        //Quando si fa ExecuteBindFillList, le date che sul db sono NULL vengono mappate con il valore default(DateTime)
        //che dovrebbe valere 01/01/0001
        public static bool IsDateDbNull(this DateTime dt)
        {
            bool res = true;
            if (dt != null && dt > default(DateTime)) res = false;
            return res;
        }

        /// <summary>
        /// Utilizzo: List<T> list = Utils.DistinctBy(listaoriginale, X=> new { X.COGNOME, X.NOME }).ToList();
        /// con X. campi del tipo della lista originale
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static string DisplayObjectInfo(Object o)
        {
            StringBuilder sb = new StringBuilder();

            // Include the type of the object
            System.Type type = o.GetType();
            sb.Append("Type: " + type.Name);

            // Include information for each Field
            sb.Append("\r\n\r\nFields:");
            System.Reflection.FieldInfo[] fi = type.GetFields();
            if (fi.Length > 0)
            {
                foreach (FieldInfo f in fi)
                {
                    sb.Append("\r\n " + f.ToString() + " = " + f.GetValue(o));
                }
            }
            else
                sb.Append("\r\n None");

            // Include information for each Property
            sb.Append("\r\n\r\nProperties:");
            System.Reflection.PropertyInfo[] pi = type.GetProperties();
            if (pi.Length > 0)
            {
                foreach (PropertyInfo p in pi)
                {
                    sb.Append("\r\n " + p.ToString() + " = " +
                              p.GetValue(o, null));
                }
            }
            else
                sb.Append("\r\n None");

            return sb.ToString();
        }

        public static string RimuoviDoppiSpazi(string stringa)
        {
            string Return = stringa.TrimStart().TrimEnd();
            try
            {
                while (Return.IndexOf("  ") > 0)
                {
                    Return = Return.Replace("  ", " ");
                }
            }
            catch
            {
            }

            return Return;
        }

        public static bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }
    }

    public static class UtilsCalc
    {
        public static double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }

        /// <summary>
        /// la formula è del tipo [FASI LIKE 'SABREV%'] AND [FORMA='TONDO'] AND [SEZIONE=130]
        /// ogni parte a sx di ogni simbolo (<>=) è un campo della tabella
        /// ogni parte a dx è un valore
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        public static DataTable GetDataTableCalcFormula(string formula)
        {
            string formulainput = formula;

            formula = formula.ToUpper().Replace(',', '.');

            DataTable tblFields = new DataTable();
            try
            {
                int startidx = 0;
                int idx1 = formula.IndexOf("[", startidx);
                while (idx1 >= 0)
                {
                    startidx = idx1;
                    int idx2 = formula.IndexOf("]", startidx);
                    int idxop = formula.IndexOf("=", startidx);
                    if (idxop > 0 && formula.IndexOf(">", startidx) < idxop) idxop = formula.IndexOf(">", startidx);
                    if (idxop > 0 && formula.IndexOf("<", startidx) < idxop) idxop = formula.IndexOf("<", startidx);
                    if (idxop > 0 && formula.IndexOf(" LIKE ", startidx) < idxop) idxop = formula.IndexOf("<", startidx);

                    bool isstring = formula.Substring(idxop + 1, idx2 - idxop).Contains("'") ||
                        formula.Substring(idxop + 1, idx2 - idxop).Contains("\"");

                    DataColumn col = new DataColumn();
                    col.ColumnName = formula.Substring(idx1 + 1, idxop - idx1 - 1);
                    col.DataType = (isstring) ? System.Type.GetType("System.String") : System.Type.GetType("System.Decimal");
                    tblFields.Columns.Add(col);

                    idx1 = formula.IndexOf("[", startidx + 1);
                }

                formula = formula.Replace("[", "");
                formula = formula.Replace("]", "");

                DataColumn colf = new DataColumn();
                colf.DataType = System.Type.GetType("System.Boolean");
                colf.ColumnName = "FORMULA";
                colf.Expression = formula;
                tblFields.Columns.Add(colf);

                /*DataRow r = tblFields.NewRow();
                foreach (NFA_CHIM el in lstel)
                {
                    r[el.ELEMENT.Trim()] = el.VAL;
                }
                tblFields.Rows.Add(r);

                res = Convert.ToBoolean(tblFields.Rows[0]["FORMULA"]);*/
            }
            catch (Exception ex)
            {
                throw;
            }
            return tblFields;
        }

        public static bool GetResCalcFormula(string formula, List<Tuple<string, string, Type>> lstFields, out string exception)
        {
            bool res = false;
            exception = "";
            string formulainput = formula;

            formula = formula.ToUpper().Replace(',', '.');

            DataTable tblFields = new DataTable();
            try
            {
                foreach (Tuple<string, string, Type> t in lstFields)
                {
                    DataColumn col = new DataColumn();
                    col.ColumnName = t.Item1;
                    col.DataType = t.Item3;
                    tblFields.Columns.Add(col);
                }

                formula = formula.Replace("[", "");
                formula = formula.Replace("]", "");

                DataColumn colf = new DataColumn();
                colf.DataType = System.Type.GetType("System.Boolean");
                colf.ColumnName = "FORMULA";
                colf.Expression = formula;
                tblFields.Columns.Add(colf);

                DataRow r = tblFields.NewRow();
                foreach (Tuple<string, string, Type> t in lstFields)
                {
                    if (t.Item3 == System.Type.GetType("System.String"))
                        r[t.Item1] = t.Item2;
                    if (t.Item3 == System.Type.GetType("System.Single"))
                        r[t.Item1] = Convert.ToSingle(t.Item2);
                    if (t.Item3 == System.Type.GetType("System.Double"))
                        r[t.Item1] = Convert.ToDouble(t.Item2);
                    if (t.Item3 == System.Type.GetType("System.Int32"))
                        r[t.Item1] = Convert.ToInt32(t.Item2);
                    if (t.Item3 == System.Type.GetType("System.Boolean"))
                        r[t.Item1] = Convert.ToBoolean(t.Item2);
                }
                tblFields.Rows.Add(r);

                res = Convert.ToBoolean(tblFields.Rows[0]["FORMULA"]);
            }
            catch (Exception ex)
            {
                //log.Error("EXCEPTION GetResCalcFormula: " + ex.Message);
                exception = ex.Message;
            }
            finally { tblFields.Dispose(); }
            return res;
        }

    }

    public static class UtilsAssembly
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetAssemblyDirectory()
        {
            try
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
            catch { return null; }
        }

        public static string GetDllsVersion()
        {
            string res = "Assembly and Dll referenced from application: ";
            try
            {
                Assembly a = Assembly.GetCallingAssembly();
                List<Assembly> lst = new List<Assembly>();
                int level = 0;
                bool b = GetAllAssemblies(a, ref lst, level, 2);
                lst = lst.OrderBy(X => X.FullName).ToList();
                int i = 1;
                //foreach (AssemblyName an in a.GetReferencedAssemblies())
                foreach (Assembly an in lst)
                {
                    res = res + "\n" + string.Format("  " + i.ToString().PadLeft(3, ' ') + ") {0}, Version={1}", an.GetName().Name, an.GetName().Version);
                    i++;
                }

                return res;
            }
            catch { return null; }
        }
        public static string GetAssemblyVersion()
        {
            AssemblyInformationalVersionAttribute infoVersion = (AssemblyInformationalVersionAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault();
            return infoVersion.InformationalVersion;
        }
        private static bool GetAllAssemblies(Assembly rootAssembly, ref List<Assembly> res, int level, int maxlevel)
        {
            bool end = false;
            int lev = ++level;

            try
            {
                if (level <= maxlevel)
                {
                    foreach (AssemblyName an in rootAssembly.GetReferencedAssemblies())
                    {
                        Assembly a = Assembly.Load(an);
                        if (a != null && res.Where(X => X.FullName == a.FullName).Count() == 0)
                            res.Add(a);

                        end = GetAllAssemblies(a, ref res, lev, maxlevel);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static List<Assembly> GetOracleAssemblies()
        {
            List<Assembly> res = new List<Assembly>();
            List<Assembly> lst = new List<Assembly>();
            try
            {
                Assembly rootAssembly = Assembly.GetCallingAssembly();
                bool resok = GetAllAssemblies(rootAssembly, ref lst, 0, 5);

                /*for (int i = 0; i < 5; i++)
                {           
      
                    foreach (AssemblyName an in rootAssembly.GetReferencedAssemblies())
                    {
                        Assembly a = Assembly.Load(an);
                        if (a != null && res.Where(X => X.FullName == a.FullName).Count() == 0)
                            res.Add(a);

                        end = GetAllAssemblies(a, ref res, lev, maxlevel);
                    }
                    
                }*/
                lst = lst.Where(X => X.FullName.ToUpper().Contains("ORACLE")).ToList();
                foreach (Assembly an in lst)
                {
                    //Assembly a = Assembly.Load(an.FullName);
                    if (an != null && res.Where(X => X.FullName == an.FullName).Count() == 0)
                        res.Add(an);
                }
            }
            catch
            {
            }
            return res;
        }

        public static List<string> GetLocalIPAddress()
        {
            List<string> res = new List<string>();
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        res.Add(ip.ToString());
                    }
                }
                //throw new Exception("No network adapters with an IPv4 address in the system!");
            }
            catch { }
            return res;
        }

        public static void AddUpdateAppSettings(string key, string value, string pathfile = "")
        {
            try
            {
                //var configFile = new Configuration();
                Configuration? configFile = null;
                if (!string.IsNullOrEmpty(pathfile))
                {
                    ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                    fileMap.ExeConfigFilename = pathfile;
                    configFile = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);// ConfigurationManager.OpenExeConfiguration(pathfile);
                }
                else configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (configFile != null)
                {
                    var settings = configFile.AppSettings.Settings;
                    if (settings[key] == null)
                    {
                        settings.Add(key, value);
                    }
                    else
                    {
                        settings[key].Value = value;
                    }
                    configFile.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                }
            }
            catch (ConfigurationErrorsException)
            {
                //log.Error("Error writing app settings");
                throw;
            }
        }

        public static void DelAppSettings(string key)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] != null)
                {
                    configFile.AppSettings.Settings.Remove(key);
                    configFile.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                }
            }
            catch (ConfigurationErrorsException)
            {
                //log.Error("Error writing app settings");
                throw;
            }
        }

        /// <summary>
        /// Utile per recuperare info da section (es: log4net) e key (es: file) all'interno di section
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetParameterAppConfig(string section, string key, string pathfile)
        {
            string res = "";

            Configuration configFile = null;
            if (!string.IsNullOrEmpty(pathfile))
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = pathfile;
                configFile = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);// ConfigurationManager.OpenExeConfiguration(pathfile);
            }
            else configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (configFile != null)
            {
                //configFile.GetSection
                var sec = (AppSettingsSection)configFile.GetSection(section);
                if (sec != null && sec.Settings[key] != null)
                    res = sec.Settings[key].ToString();
            }
            return res;
        }

        public static string GetPathLogConfigLog4net()
        {
            var appenders = LogManager.GetRepository().GetAppenders();
            var filePath = appenders.OfType<FileAppender>().Single().File;
            return filePath;
        }

        public static void LogError(Exception e)
        {
            log.Error(e.StackTrace.ToString());
            log.Error(e.Message);
        }

    }

    public class AppDomain
    {
        public static AppDomain CurrentDomain { get; private set; }

        static AppDomain()
        {
            CurrentDomain = new AppDomain();
        }

        public Assembly[] GetAssemblies()
        {
            var assemblies = new List<Assembly>();
            var dependencies = DependencyContext.Default.RuntimeLibraries;
            foreach (var library in dependencies)
            {
                if (IsCandidateCompilationLibrary(library))
                {
                    var assembly = Assembly.Load(new AssemblyName(library.Name));
                    assemblies.Add(assembly);
                }
            }
            return assemblies.ToArray();
        }

        private static bool IsCandidateCompilationLibrary(RuntimeLibrary compilationLibrary)
        {
            return compilationLibrary.Name == ("Specify")
                || compilationLibrary.Dependencies.Any(d => d.Name.StartsWith("Specify"));
        }
    }
}
