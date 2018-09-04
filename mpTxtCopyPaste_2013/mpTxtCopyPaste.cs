using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mpTxtCopyPaste
{
    public class MpTxtCopyPaste
    {
        private const string LangItem = "mpTxtCopyPaste";

        [CommandMethod("ModPlus", "mpTxtCopyPaste", CommandFlags.UsePickSet)]
        public static void MainFunction()
        {
            Statistic.SendCommandStarting(new Interface());

            try
            {
                var deleteSource = false; // Удаление исходника
                // Значение по-умолчанию из расширенных данных чертеже
                var defVal = ModPlus.Helpers.XDataHelpers.GetStringXData("mpTxtCopyPaste");
                if (!string.IsNullOrEmpty(defVal))
                {
                    if (bool.TryParse(defVal, out var tmp))
                        deleteSource = tmp;
                }
                // 
                var doc = AcApp.DocumentManager.MdiActiveDocument;
                var db = doc.Database;
                var ed = doc.Editor;

                var peo = new PromptEntityOptions("\n" + Language.GetItem(LangItem, "msg1"));
                peo.SetMessageAndKeywords("\n" + Language.GetItem(LangItem, "msg2"), "Delete");
                peo.AppendKeywordsToMessage = true;
                peo.SetRejectMessage("\n" + Language.GetItem(LangItem, "msg3"));
                peo.AddAllowedClass(typeof(DBText), false);
                peo.AddAllowedClass(typeof(MText), false);
                peo.AllowNone = true;
                var per = ed.GetEntity(peo);
                if (per.Status == PromptStatus.Keyword)
                {
                    deleteSource = MessageBox.ShowYesNo(Language.GetItem(LangItem, "msg4"), MessageBoxIcon.Question);
                    // Сохраняем текущее значение как значение по умолчанию
                    using (doc.LockDocument())
                    {
                        ModPlus.Helpers.XDataHelpers.SetStringXData("mpTxtCopyPaste", deleteSource.ToString());
                    }
                }
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
                            while (true)
                            {
                                peo = new PromptEntityOptions("\n" + Language.GetItem(LangItem, "msg5"));
                                peo.SetRejectMessage("\n" + Language.GetItem(LangItem, "msg3"));
                                peo.AddAllowedClass(typeof(DBText), false);
                                peo.AddAllowedClass(typeof(MText), false);
                                peo.AllowNone = false;
                                per = ed.GetEntity(peo);
                                if (per.Status != PromptStatus.OK)
                                    break;
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
                                db.TransactionManager.QueueForGraphicsFlush();
                            }
                            // Delete source
                            if (deleteSource)
                                ent.Erase(true);
                            tr.Commit();
                        }
                    }
                }
            }
            catch (System.Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
    }
}
