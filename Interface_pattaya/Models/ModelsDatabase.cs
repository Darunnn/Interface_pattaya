using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface_pattaya.Models
{
    public class PrescriptionBodyRequest
    {
        public string UniqID { get; set; }
        public string f_prescriptionno { get; set; }
        public int f_seq { get; set; }
        public int f_seqmax { get; set; }
        public string f_prescriptiondate { get; set; }
        public string f_ordercreatedate { get; set; }
        public string f_ordertargetdate { get; set; }
        public string f_ordertargettime { get; set; }
        public string f_doctorcode { get; set; }
        public string f_doctorname { get; set; }
        public string f_useracceptby { get; set; }
        public string f_orderacceptdate { get; set; }
        public string f_orderacceptfromip { get; set; }
        public string f_pharmacylocationcode { get; set; }
        public string f_pharmacylocationdesc { get; set; }
        public string f_prioritycode { get; set; }
        public string f_prioritydesc { get; set; }
        public string f_hn { get; set; }
        public string f_an { get; set; }
        public string f_vn { get; set; }
        public string f_title { get; set; }
        public string f_patientname { get; set; }
        public string f_sex { get; set; }
        public string f_patientdob { get; set; }
        public string f_wardcode { get; set; }
        public string f_warddesc { get; set; }
        public string f_roomcode { get; set; }
        public string f_roomdesc { get; set; }
        public string f_bedcode { get; set; }
        public string f_beddesc { get; set; }
        public string f_right { get; set; }
        public string f_drugallergy { get; set; }
        public string f_diagnosis { get; set; }
        public string f_orderitemcode { get; set; }
        public string f_orderitemname { get; set; }
        public string f_orderitemnameTH { get; set; }
        public string f_orderitemnamegeneric { get; set; }
        public int f_orderqty { get; set; }
        public string f_orderunitcode { get; set; }
        public string f_orderunitdesc { get; set; }
        public int f_dosage { get; set; }
        public string f_dosageunit { get; set; }
        public string f_dosagetext { get; set; }
        public string f_drugformcode { get; set; }
        public string f_drugformdesc { get; set; }
        public string f_HAD { get; set; }
        public string f_narcoticFlg { get; set; }
        public string f_psychotropic { get; set; }
        public string f_binlocation { get; set; }
        public string f_itemidentify { get; set; }
        public string f_itemlotno { get; set; }
        public string f_itemlotexpire { get; set; }
        public string f_instructioncode { get; set; }
        public string f_instructiondesc { get; set; }
        public string f_frequencycode { get; set; }
        public string f_frequencydesc { get; set; }
        public string f_timecode { get; set; }
        public string f_timedesc { get; set; }
        public string f_frequencytime { get; set; }
        public string f_dosagedispense { get; set; }
        public string f_dayofweek { get; set; }
        public string f_noteprocessing { get; set; }
        public string f_prn { get; set; }
        public string f_stat { get; set; }
        public string f_comment { get; set; }
        public int f_tomachineno { get; set; }
        public string f_ipd_order_recordno { get; set; }
        public string f_status { get; set; }
        public string f_remark { get; set; }
        public string f_symptom { get; set; }
        public string f_durationtext { get; set; }
        public string f_dosagedispense_compare { get; set; }
    }

    public class PrescriptionBodyResponse
    {
        public PrescriptionBodyRequest[] data { get; set; }
    }

    public class GridViewDataModel
    {
        public string PrescriptionNo { get; set; }
        public string Seq { get; set; }
        public string SeqMax { get; set; }
        public string PatientName { get; set; }
        public string HN { get; set; }
        public string ItemNameTH { get; set; }
        public string OrderQty { get; set; }
        public string OrderUnit { get; set; }
        public string Dosage { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
    }
}