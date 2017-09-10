using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using mpMsg;
using ModPlus;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace mpTxtCopyPaste
{
    public class MpTxtCopyPaste
    {
        [CommandMethod("ModPlus", "mpTxtCopyPaste", CommandFlags.UsePickSet)]
        public static void MainFunction()
        {
            try
            {
                var keepLoopin = true; // Для цикличного выполнения функции
                var deleteSource = false; // Удаление исходника
                // Значение по-умолчанию из расширенных данных чертеже
                var defVal = MpCadHelpers.GetStringXData("mpTxtCopyPaste");
                if (!string.IsNullOrEmpty(defVal))
                {
                    bool tmp;
                    if (bool.TryParse(defVal, out tmp))
                        deleteSource = tmp;
                }
                // 
                var doc = AcApp.DocumentManager.MdiActiveDocument;
                var db = doc.Database;
                var ed = doc.Editor;

                var peo = new PromptEntityOptions("\nВыберите текст-исходник:");
                while (keepLoopin)
                {
                    peo.SetMessageAndKeywords("\nВыберите текст-исходник или [Удалять]", "Delete");
                    peo.AppendKeywordsToMessage = true;
                    peo.SetRejectMessage("\nНеверный выбор!");
                    peo.AddAllowedClass(typeof(DBText), false);
                    peo.AddAllowedClass(typeof(MText), false);
                    peo.AllowNone = true;
                    var per = ed.GetEntity(peo);
                    if (per.Status == PromptStatus.Keyword)
                    {
                        deleteSource = MpQstWin.Show("Удалять текст-исходник?");
                        // Сохраняем текущее значение как значение по умолчанию
                        using (doc.LockDocument())
                        {
                            MpCadHelpers.SetStringXData("mpTxtCopyPaste", deleteSource.ToString());
                        }
                    }
                    else if (
                        per.Status == PromptStatus.None ||
                        per.Status == PromptStatus.Error ||
                        per.Status == PromptStatus.Cancel)
                        keepLoopin = false;
                    else if (per.Status == PromptStatus.OK)
                    {
                        using (doc.LockDocument())
                        {
                            using (var tr = db.TransactionManager.StartTransaction())
                            {
                                var ent = (Entity)tr.GetObject(per.ObjectId, OpenMode.ForWrite);
                                var str = string.Empty;
                                // Если выбранный примитив - однострочный текст
                                if (ent is DBText)
                                {
                                    var txt = (DBText)ent;
                                    str = txt.TextString;
                                }
                                // Если выбранный примитив - многострочный текст
                                else if (ent is MText)
                                {
                                    var txt = (MText)ent;
                                    str = txt.Contents;
                                }
                                peo = new PromptEntityOptions("\nВыберите текст для замены содержимого");
                                peo.SetRejectMessage("\nНеверный выбор!");
                                peo.AddAllowedClass(typeof(DBText), false);
                                peo.AddAllowedClass(typeof(MText), false);
                                peo.AllowNone = false;
                                per = ed.GetEntity(peo);
                                if (per.Status != PromptStatus.OK) continue;
                                var selectedEnt = (Entity)tr.GetObject(per.ObjectId, OpenMode.ForWrite);
                                if (selectedEnt is DBText)
                                {
                                    var selectedText = (DBText)selectedEnt;
                                    selectedText.UpgradeOpen();
                                    selectedText.TextString = str;
                                    selectedText.DowngradeOpen();
                                }
                                if (selectedEnt is MText)
                                {
                                    var selectedText = (MText)selectedEnt;
                                    selectedText.UpgradeOpen();
                                    selectedText.Contents = str;
                                    selectedText.DowngradeOpen();
                                }
                                // Delete source
                                if (deleteSource)
                                    ent.Erase(true);
                                tr.Commit();
                            }
                        }
                    }
                }
            }
            catch (System.Exception exception)
            {
                MpExWin.Show(exception);
            }
        }
    }
}
