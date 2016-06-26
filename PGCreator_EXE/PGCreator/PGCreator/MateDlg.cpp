// MateDlg.cpp : ʵ���ļ�
//

#include "stdafx.h"
#include "PGCreator.h"
#include "PGCreatorDlg.h"
#include "MateDlg.h"
#include "afxdialogex.h"

CString pgMaterial[NUM_MATERIAL] = {L"������",L"ԤӦ����",L"��ͨ�ֲ�",L"���Ӹ�",L"���Ƹ�",L"��-��������",L"����-����������",L"����-��Ͳ������",L"����",L"ľ��",L"����",L"ʯ���",L"��ֽ",L"��ש",L"����ʯ",L"����Ƭ",L"�����Ƭ"};
CString default_zh[NUM_MATERIAL] = {L"������",L"ԤӦ����",L"��",L"���Ӹ�",L"���Ƹ�",L"���������˸ֹ�",L"����������������",L"����Ͳ����������",L"����",L"ľ��",L"����",L"ʯ��",L"��ֽ",L"��ש",L"����ʯ",L"����Ƭ",L"�����Ƭ"};
CString default_en[NUM_MATERIAL] = {L"Concrete",L"Prestress Concrete",L"Steel",L"Welded Steel",L"Threaded Steel",L"Vitaulic Steel",L"Cast Iron w/flexible couplings",L"Cast Iron w/bell and spigot couplings",L"Masonry",L"Wood",L"Glass",L"Gypsum",L"Wallpaper",L"Ceramic",L"Marble",L"Concrete Tile",L"Clay Tile"};
CString byUser[NUM_MATERIAL];

// CMateDlg �Ի���

IMPLEMENT_DYNAMIC(CMateDlg, CDialog)

CMateDlg::CMateDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CMateDlg::IDD, pParent)
	, m_Emate(_T(""))
{
	for (int i=0;i<NUM_MATERIAL;++i)
	{
		byUser[i] = default_en[i];
	}
}

CMateDlg::~CMateDlg()
{
}

void CMateDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_MATE, m_Emate);
	DDV_MaxChars(pDX, m_Emate,MAX_MATERIAL_LENGTH);
}


BEGIN_MESSAGE_MAP(CMateDlg, CDialog)
	ON_NOTIFY(NM_CLICK, IDC_LIST_MATE, &CMateDlg::OnClickListMate)
	ON_EN_KILLFOCUS(IDC_EDIT_MATE, &CMateDlg::OnKillfocusEditMate)
	ON_BN_CLICKED(IDC_BUTTON_DEFAULT_EN, &CMateDlg::OnClickedButtonDefaultEn)
	ON_BN_CLICKED(IDC_BUTTON_DEFAULT_ZH, &CMateDlg::OnClickedButtonDefaultZh)
END_MESSAGE_MAP()


// CMateDlg ��Ϣ�������


BOOL CMateDlg::OnInitDialog()
{
	CListCtrl* p_Lmate = (CListCtrl*)GetDlgItem(IDC_LIST_MATE);
	CEdit* p_Emate = (CEdit*)GetDlgItem(IDC_EDIT_MATE);
	DWORD dwStyle = p_Lmate->GetExtendedStyle();
	dwStyle |= LVS_EX_FULLROWSELECT;
	dwStyle |= LVS_EX_GRIDLINES;

	p_Lmate->SetExtendedStyle(dwStyle); 
	p_Lmate->InsertColumn(0,L"empty", LVCFMT_CENTER,90);
	p_Lmate->InsertColumn(1,L"PG����", LVCFMT_CENTER,100);
	p_Lmate->InsertColumn(2,L"Revit��������", LVCFMT_CENTER,297);
	p_Lmate->DeleteColumn(0);

	p_Lmate->GetHeaderCtrl()->EnableWindow(FALSE);

	for (int i=NUM_MATERIAL-1;0<=i;--i)
	{
		p_Lmate->InsertItem(0,pgMaterial[i]);
		p_Lmate->SetItemText(0,1,byUser[i]);
	}

	p_Emate->ShowWindow(SW_HIDE);

	return TRUE;
}


void CMateDlg::OnClickListMate(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMITEMACTIVATE pNMItemActivate = reinterpret_cast<LPNMITEMACTIVATE>(pNMHDR);
	*pResult = 0;
	CListCtrl* p_Lmate = (CListCtrl*)GetDlgItem(IDC_LIST_MATE);
	CEdit* p_Emate = (CEdit*)GetDlgItem(IDC_EDIT_MATE);
	currItem = pNMItemActivate->iItem;
	currSubItem = pNMItemActivate->iSubItem;
	if (currItem == -1 || currSubItem != 1)	return;
	CRect rect_dlg,rect_list,rect_edit;
	POINT point;
	GetWindowRect(rect_dlg);
	p_Lmate->GetWindowRect(rect_list);
	p_Lmate->GetSubItemRect(currItem, currSubItem, LVIR_LABEL, rect_edit);
	point.x = rect_list.left - rect_dlg.left;
	point.y = rect_list.top - rect_dlg.top;
	rect_edit.OffsetRect(point);
	rect_edit.bottom += 2;
	p_Emate->ShowWindow(SW_SHOW);
	p_Emate->MoveWindow(&rect_edit, TRUE);
	p_Emate->SetFocus();
	p_Lmate->SetItemText(currItem,currSubItem,L" ");
}


void CMateDlg::OnKillfocusEditMate()
{
	// TODO: �ڴ���ӿؼ�֪ͨ����������
	CListCtrl* p_Lmate = (CListCtrl*)GetDlgItem(IDC_LIST_MATE);
	CEdit* p_Emate = (CEdit*)GetDlgItem(IDC_EDIT_MATE);
	CString temp;
	p_Emate->ShowWindow(SW_HIDE);
	p_Emate->GetWindowTextW(temp);
	if (!temp.IsEmpty())
	{
		if (UpdateData(TRUE))
		{
			byUser[currItem] = m_Emate;
		}
	}
	p_Lmate->SetItemText(currItem,currSubItem,byUser[currItem]);
	p_Emate->SetWindowTextW(L"\0");
}


BOOL CMateDlg::PreTranslateMessage(MSG* pMsg)
{
	// TODO: �ڴ����ר�ô����/����û���
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_ESCAPE ) return TRUE;
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_RETURN ) return TRUE;
	else return CDialog::PreTranslateMessage(pMsg);
}

void CMateDlg::OutputInfo(FILE* fp)
{
	char temp[MAX_MATERIAL_LENGTH];
	for (int i=0;i<NUM_MATERIAL;++i)
	{
		CPGCreatorDlg::TcharToChar(byUser[i],temp);
		fprintf_s(fp,"%s\t",temp);
	}
	fprintf_s(fp,"\n");
}  

void CMateDlg::OnClickedButtonDefaultEn()
{
	// TODO: �ڴ���ӿؼ�֪ͨ����������
	CListCtrl* p_Lmate = (CListCtrl*)GetDlgItem(IDC_LIST_MATE);
	for (int i=0;i<NUM_MATERIAL;++i)
	{
		byUser[i] = default_en[i];
		p_Lmate->SetItemText(i,1,byUser[i]);
	}
}


void CMateDlg::OnClickedButtonDefaultZh()
{
	// TODO: �ڴ���ӿؼ�֪ͨ����������
	CListCtrl* p_Lmate = (CListCtrl*)GetDlgItem(IDC_LIST_MATE);
	for (int i=0;i<NUM_MATERIAL;++i)
	{
		byUser[i] = default_zh[i];
		p_Lmate->SetItemText(i,1,byUser[i]);
	}
}
