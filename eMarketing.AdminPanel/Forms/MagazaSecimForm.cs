using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Forms
{
    public class MagazaSecimForm : Form
    {
        private readonly MagazaRepository _repo = new MagazaRepository();

        public bool SecimYapildi { get; private set; }

        // devam eden kodlar burada kalacak
    }
}