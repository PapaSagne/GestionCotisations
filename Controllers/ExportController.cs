using ClosedXML.Excel;
using GestionCotisations.Web.Data;
using GestionCotisations.Web.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GestionCotisations.Web.Controllers
{
    [Authorize]
    public class ExportController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static readonly BaseColor PdfBlanc = new BaseColor(255, 255, 255);
        private static readonly BaseColor PdfNoir = new BaseColor(0, 0, 0);
        private static readonly BaseColor PdfGris = new BaseColor(128, 128, 128);
        private static readonly BaseColor PdfBleuFonce = new BaseColor(30, 58, 95);
        private static readonly BaseColor PdfBleuClair = new BaseColor(45, 106, 159);
        private static readonly BaseColor PdfGrisClaire = new BaseColor(240, 244, 248);
        private static readonly BaseColor PdfVert = new BaseColor(40, 167, 69);

        public ExportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== CSV ====================

        public async Task<IActionResult> MembresCsv()
        {
            var membres = await _context.Membres.OrderBy(m => m.Nom).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id;Nom;Prénom;Email;Téléphone;Adresse;Date Inscription;Statut");

            foreach (var m in membres)
            {
                sb.AppendLine($"{m.Id};{m.Nom};{m.Prenom};{m.Email};{m.Telephone};{m.Adresse};" +
                              $"{m.DateInscription:dd/MM/yyyy};{(m.EstActif ? "Actif" : "Inactif")}");
            }

            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

            return File(bytes, "text/csv", $"membres_{DateTime.Now:yyyyMMdd}.csv");
        }

        public async Task<IActionResult> CotisationsCsv()
        {
            var cotisations = await _context.Cotisations
                .Include(c => c.Membre)
                .OrderByDescending(c => c.DateEcheance)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id;Membre;Libellé;Montant;Date Début;Date Fin;Échéance;Statut");

            foreach (var c in cotisations)
            {
                sb.AppendLine($"{c.Id};{c.Membre?.Prenom} {c.Membre?.Nom};{c.Libelle};" +
                              $"{c.Montant};{c.DateDebut:dd/MM/yyyy};{c.DateFin:dd/MM/yyyy};" +
                              $"{c.DateEcheance:dd/MM/yyyy};{c.Statut}");
            }

            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

            return File(bytes, "text/csv", $"cotisations_{DateTime.Now:yyyyMMdd}.csv");
        }

        public async Task<IActionResult> PaiementsCsv()
        {
            var paiements = await _context.Paiements
                .Include(p => p.Cotisation).ThenInclude(c => c.Membre)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id;Membre;Cotisation;Montant;Date;Mode;Référence;Remarque");

            foreach (var p in paiements)
            {
                sb.AppendLine($"{p.Id};{p.Cotisation?.Membre?.Prenom} {p.Cotisation?.Membre?.Nom};" +
                              $"{p.Cotisation?.Libelle};{p.Montant};{p.DatePaiement:dd/MM/yyyy};" +
                              $"{p.Mode};{p.Reference};{p.Remarque}");
            }

            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

            return File(bytes, "text/csv", $"paiements_{DateTime.Now:yyyyMMdd}.csv");
        }

        // ==================== EXCEL ====================

        public async Task<IActionResult> RapportExcel()
        {
            var membres = await _context.Membres.OrderBy(m => m.Nom).ToListAsync();
            var cotisations = await _context.Cotisations
                .Include(c => c.Membre).OrderByDescending(c => c.DateEcheance).ToListAsync();
            var paiements = await _context.Paiements
                .Include(p => p.Cotisation).ThenInclude(c => c.Membre)
                .OrderByDescending(p => p.DatePaiement).ToListAsync();

            using var workbook = new XLWorkbook();

            var wsMembres = workbook.Worksheets.Add("Membres");
            wsMembres.Cell(1, 1).Value = "LISTE DES MEMBRES";
            wsMembres.Range(1, 1, 1, 6).Merge().Style
                .Font.SetBold(true).Font.SetFontSize(14)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#1e3a5f"))
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            string[] headersMembres = { "Nom", "Prénom", "Email", "Téléphone", "Inscription", "Statut" };
            for (int i = 0; i < headersMembres.Length; i++)
            {
                var cell = wsMembres.Cell(2, i + 1);
                cell.Value = headersMembres[i];
                cell.Style.Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#2d6a9f"))
                    .Font.SetFontColor(XLColor.White);
            }

            for (int i = 0; i < membres.Count; i++)
            {
                var m = membres[i];
                var row = i + 3;
                wsMembres.Cell(row, 1).Value = m.Nom;
                wsMembres.Cell(row, 2).Value = m.Prenom;
                wsMembres.Cell(row, 3).Value = m.Email;
                wsMembres.Cell(row, 4).Value = m.Telephone;
                wsMembres.Cell(row, 5).Value = m.DateInscription.ToString("dd/MM/yyyy");
                wsMembres.Cell(row, 6).Value = m.EstActif ? "Actif" : "Inactif";
                if (i % 2 == 0)
                    wsMembres.Row(row).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#f0f4f8"));
            }
            wsMembres.Columns().AdjustToContents();

            var wsCotisations = workbook.Worksheets.Add("Cotisations");
            wsCotisations.Cell(1, 1).Value = "LISTE DES COTISATIONS";
            wsCotisations.Range(1, 1, 1, 5).Merge().Style
                .Font.SetBold(true).Font.SetFontSize(14)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#1e3a5f"))
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            string[] headersCot = { "Membre", "Libellé", "Montant", "Échéance", "Statut" };
            for (int i = 0; i < headersCot.Length; i++)
            {
                var cell = wsCotisations.Cell(2, i + 1);
                cell.Value = headersCot[i];
                cell.Style.Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#2d6a9f"))
                    .Font.SetFontColor(XLColor.White);
            }

            for (int i = 0; i < cotisations.Count; i++)
            {
                var c = cotisations[i];
                var row = i + 3;
                wsCotisations.Cell(row, 1).Value = $"{c.Membre?.Prenom} {c.Membre?.Nom}";
                wsCotisations.Cell(row, 2).Value = c.Libelle;
                wsCotisations.Cell(row, 3).Value = (double)c.Montant;
                wsCotisations.Cell(row, 3).Style.NumberFormat.Format = "#,##0";
                wsCotisations.Cell(row, 4).Value = c.DateEcheance.ToString("dd/MM/yyyy");
                wsCotisations.Cell(row, 5).Value = c.Statut.ToString();
                if (i % 2 == 0)
                    wsCotisations.Row(row).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#f0f4f8"));
            }

            var totalRow = cotisations.Count + 3;
            wsCotisations.Cell(totalRow, 2).Value = "TOTAL";
            wsCotisations.Cell(totalRow, 2).Style.Font.SetBold(true);
            wsCotisations.Cell(totalRow, 3).Value = (double)cotisations.Sum(c => c.Montant);
            wsCotisations.Cell(totalRow, 3).Style.Font.SetBold(true).NumberFormat.Format = "#,##0";
            wsCotisations.Columns().AdjustToContents();

            var wsPaiements = workbook.Worksheets.Add("Paiements");
            wsPaiements.Cell(1, 1).Value = "LISTE DES PAIEMENTS";
            wsPaiements.Range(1, 1, 1, 6).Merge().Style
                .Font.SetBold(true).Font.SetFontSize(14)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#1e3a5f"))
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            string[] headersPai = { "Membre", "Cotisation", "Montant", "Date", "Mode", "Référence" };
            for (int i = 0; i < headersPai.Length; i++)
            {
                var cell = wsPaiements.Cell(2, i + 1);
                cell.Value = headersPai[i];
                cell.Style.Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#2d6a9f"))
                    .Font.SetFontColor(XLColor.White);
            }

            for (int i = 0; i < paiements.Count; i++)
            {
                var p = paiements[i];
                var row = i + 3;
                wsPaiements.Cell(row, 1).Value = $"{p.Cotisation?.Membre?.Prenom} {p.Cotisation?.Membre?.Nom}";
                wsPaiements.Cell(row, 2).Value = p.Cotisation?.Libelle;
                wsPaiements.Cell(row, 3).Value = (double)p.Montant;
                wsPaiements.Cell(row, 3).Style.NumberFormat.Format = "#,##0";
                wsPaiements.Cell(row, 4).Value = p.DatePaiement.ToString("dd/MM/yyyy");
                wsPaiements.Cell(row, 5).Value = p.Mode.ToString();
                wsPaiements.Cell(row, 6).Value = p.Reference;
                if (i % 2 == 0)
                    wsPaiements.Row(row).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#f0f4f8"));
            }

            var totalPaiRow = paiements.Count + 3;
            wsPaiements.Cell(totalPaiRow, 2).Value = "TOTAL";
            wsPaiements.Cell(totalPaiRow, 2).Style.Font.SetBold(true);
            wsPaiements.Cell(totalPaiRow, 3).Value = (double)paiements.Sum(p => p.Montant);
            wsPaiements.Cell(totalPaiRow, 3).Style.Font.SetBold(true).NumberFormat.Format = "#,##0";
            wsPaiements.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"rapport_cotisations_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // ==================== PDF ====================

        public async Task<IActionResult> MembresPdf()
        {
            var membres = await _context.Membres.OrderBy(m => m.Nom).ToListAsync();

            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4.Rotate(), 20, 20, 30, 30);
            PdfWriter.GetInstance(document, stream);
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, PdfBleuFonce);
            document.Add(new Paragraph("LISTE DES MEMBRES", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 5
            });

            var subFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, PdfGris);
            document.Add(new Paragraph($"Généré le {DateTime.Now:dd/MM/yyyy à HH:mm}", subFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            });

            var table = new PdfPTable(6) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 1.5f, 1.5f, 2.5f, 1.5f, 1.5f, 1f });

            string[] headers = { "Nom", "Prénom", "Email", "Téléphone", "Inscription", "Statut" };
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, PdfBlanc);

            foreach (var h in headers)
            {
                table.AddCell(new PdfPCell(new Phrase(h, headerFont))
                {
                    BackgroundColor = PdfBleuClair,
                    Padding = 8,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            }

            var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, PdfNoir);
            var dataFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, PdfNoir);

            for (int i = 0; i < membres.Count; i++)
            {
                var m = membres[i];
                var bg = i % 2 == 0 ? PdfGrisClaire : PdfBlanc;

                void AddCell(PdfPCell cell)
                {
                    cell.BackgroundColor = bg;
                    cell.Padding = 6;
                    table.AddCell(cell);
                }

                AddCell(new PdfPCell(new Phrase(m.Nom, dataFontBold)));
                AddCell(new PdfPCell(new Phrase(m.Prenom, dataFont)));
                AddCell(new PdfPCell(new Phrase(m.Email, dataFont)));
                AddCell(new PdfPCell(new Phrase(m.Telephone, dataFont)));
                AddCell(new PdfPCell(new Phrase(m.DateInscription.ToString("dd/MM/yyyy"), dataFont)));

                var statutColor = m.EstActif ? PdfVert : PdfGris;
                var statutFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, statutColor);
                var c6 = new PdfPCell(new Phrase(m.EstActif ? "Actif" : "Inactif", statutFont));
                c6.HorizontalAlignment = Element.ALIGN_CENTER;
                AddCell(c6);
            }

            document.Add(table);

            var totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, PdfBleuFonce);
            document.Add(new Paragraph($"\nTotal : {membres.Count} membre(s)", totalFont));

            document.Close();

            return File(stream.ToArray(), "application/pdf",
                $"membres_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> RapportPdf()
        {
            var paiements = await _context.Paiements
                .Include(p => p.Cotisation).ThenInclude(c => c.Membre)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();

            var totalEncaisse = paiements.Sum(p => p.Montant);
            var totalAttendu = await _context.Cotisations
                .Where(c => c.Statut != StatutCotisation.Annulee)
                .SumAsync(c => (decimal?)c.Montant) ?? 0;

            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4.Rotate(), 20, 20, 30, 30);
            PdfWriter.GetInstance(document, stream);
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, PdfBleuFonce);
            document.Add(new Paragraph("RAPPORT DES PAIEMENTS", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 5
            });

            var subFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, PdfGris);
            document.Add(new Paragraph($"Généré le {DateTime.Now:dd/MM/yyyy à HH:mm}", subFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15
            });

            var resumeTable = new PdfPTable(3) { WidthPercentage = 80, SpacingAfter = 20 };
            resumeTable.HorizontalAlignment = Element.ALIGN_CENTER;

            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, PdfBlanc);
            var valueFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, PdfBlanc);

            void AddResumeCell(string label, string value, BaseColor color)
            {
                var cell = new PdfPCell
                {
                    BackgroundColor = color,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                cell.AddElement(new Paragraph(label, boldFont));
                cell.AddElement(new Paragraph(value, valueFont));
                resumeTable.AddCell(cell);
            }

            AddResumeCell("Total attendu", $"{totalAttendu:N0} FCFA", PdfBleuFonce);
            AddResumeCell("Total encaissé", $"{totalEncaisse:N0} FCFA", PdfVert);
            AddResumeCell("Taux recouvrement",
                totalAttendu > 0 ? $"{Math.Round(totalEncaisse / totalAttendu * 100, 1)}%" : "0%",
                PdfBleuClair);

            document.Add(resumeTable);

            // Tableau paiements
            var table = new PdfPTable(6) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 2f, 2.5f, 1.5f, 1.5f, 1.5f, 1.5f });

            string[] headers = { "Membre", "Cotisation", "Montant", "Date", "Mode", "Référence" };
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, PdfBlanc);

            foreach (var h in headers)
            {
                table.AddCell(new PdfPCell(new Phrase(h, headerFont))
                {
                    BackgroundColor = PdfBleuClair,
                    Padding = 8,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            }

            var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, PdfNoir);

            for (int i = 0; i < paiements.Count; i++)
            {
                var p = paiements[i];
                var bg = i % 2 == 0 ? PdfGrisClaire : PdfBlanc;

                void AddCell(string text)
                {
                    table.AddCell(new PdfPCell(new Phrase(text, dataFont))
                    {
                        BackgroundColor = bg,
                        Padding = 6
                    });
                }

                AddCell($"{p.Cotisation?.Membre?.Prenom} {p.Cotisation?.Membre?.Nom}");
                AddCell(p.Cotisation?.Libelle ?? "");
                AddCell($"{p.Montant:N0} FCFA");
                AddCell(p.DatePaiement.ToString("dd/MM/yyyy"));
                AddCell(p.Mode.ToString());
                AddCell(p.Reference ?? "");
            }

            document.Add(table);

            var totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, PdfVert);
            document.Add(new Paragraph($"\nTotal encaissé : {totalEncaisse:N0} FCFA", totalFont)
            {
                Alignment = Element.ALIGN_RIGHT
            });

            document.Close();

            return File(stream.ToArray(), "application/pdf",
                $"rapport_paiements_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}