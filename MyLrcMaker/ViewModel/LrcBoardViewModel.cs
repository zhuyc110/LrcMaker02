﻿using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Windows.Input;
using MyLrcMaker.Model;
using Prism.Commands;
using Prism.Mvvm;

namespace MyLrcMaker.ViewModel
{
    [Export(typeof(LrcBoardViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LrcBoardViewModel : BindableBase
    {
        public ICommand OpenLrcCommand { get; }

        public ICommand SaveLrcCommand { get; }

        public ObservableCollection<ILrcModel> LrcSource { get; set; }

        [ImportingConstructor]
        public LrcBoardViewModel(ILrcManager lrcManager)
        {
            _lrcManager = lrcManager;
            OpenLrcCommand = new DelegateCommand(LoadLrc);
            SaveLrcCommand = new DelegateCommand(SaveLrc);
            LrcSource = new ObservableCollection<ILrcModel>(lrcManager.LrcModels);
        }

        #region Private methods

        private void LoadLrc()
        {
            using (var ofd = new OpenFileDialog {Filter = "歌词文件|*.txt;*.lrc", Multiselect = false, Title = "打开歌词文件"})
            {
                var result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _lrcManager.LoadLrcFromInputString(ofd.FileName);
                    LrcSource.Clear();
                    LrcSource.AddRange(_lrcManager.LrcModels);
                }
            }
        }

        private void SaveLrc()
        {
            using (var sfd = new SaveFileDialog{ Filter = "歌词文件|*.txt;*.lrc" , Title = "保存歌词文件" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    _lrcManager.SaveLrcToFile(sfd.FileName, LrcSource);
                }
            }
        }

        #endregion

        #region Fields

        private readonly ILrcManager _lrcManager;

        #endregion
    }
}