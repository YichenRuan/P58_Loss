// AddMEPDlg.cpp : 实现文件
//

#include "stdafx.h"
#include "PGCreator.h"
#include "AddMEPDlg.h"
#include "MEPDlg.h"
#include "afxdialogex.h"
#include <list>
using namespace std;


// CAddMEPDlg 对话框

IMPLEMENT_DYNAMIC(CAddMEPDlg, CDialog)

CAddMEPDlg::CAddMEPDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CAddMEPDlg::IDD, pParent)
{
}

CAddMEPDlg::~CAddMEPDlg()
{
}

void CAddMEPDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CAddMEPDlg, CDialog)
	ON_BN_CLICKED(IDOK, &CAddMEPDlg::OnBnClickedOk)
	ON_CBN_SELCHANGE(IDC_COMBO_CATECODE, &CAddMEPDlg::OnSelchangeComboCatecode)
END_MESSAGE_MAP()


// CAddMEPDlg 消息处理程序


BOOL CAddMEPDlg::OnInitDialog()
{
	CComboBox* p_Cmep = (CComboBox*)GetDlgItem(IDC_COMBO_MEPCODE);
	CComboBox* p_Ccate = (CComboBox*)GetDlgItem(IDC_COMBO_CATECODE);
	CListCtrl* p_Ltype = (CListCtrl*)GetDlgItem(IDC_LIST_MEPTYPE);
	DWORD dwStyle = p_Ltype->GetExtendedStyle();
	dwStyle |= LVS_EX_FULLROWSELECT;
	dwStyle |= LVS_EX_GRIDLINES;
	p_Ltype->SetExtendedStyle(dwStyle); 
	p_Ltype->InsertColumn(0,L"empty", LVCFMT_CENTER,50);
	p_Ltype->InsertColumn(1,L"Type", LVCFMT_CENTER,312);
	p_Ltype->DeleteColumn(0);
	for (list<CString>::iterator it = MEPComp.begin(); it != MEPComp.end(); ++ it)
	{
		p_Cmep->AddString(*it);
	}
	for (list<CString>::iterator it = cate.begin(); it != cate.end(); ++ it)
	{
		p_Ccate->AddString(*it);
	}
	p_Cmep->SetCurSel(0);
	p_Ccate->SetCurSel(0);
	list<CString> temp = type.front();
	for (list<CString>::reverse_iterator rit = temp.rbegin(); rit != temp.rend(); ++ rit)
	{
		p_Ltype->InsertItem(0,*rit);
	}

	index_MEP = index_cate = 0;
	sele_type.clear();

	return TRUE;
}

void CAddMEPDlg::Synchronize(list<CString>& MEPComp0, list<CString>& cate0, list<list<CString>>& type0)
{
	MEPComp = MEPComp0;
	cate = cate0;
	type = type0;
}

void CAddMEPDlg::OnBnClickedOk()
{
	// TODO: 在此添加控件通知处理程序代码
	CComboBox* p_Cmep = (CComboBox*)GetDlgItem(IDC_COMBO_MEPCODE);
	CComboBox* p_Ccate = (CComboBox*)GetDlgItem(IDC_COMBO_CATECODE);
	CComboBox* p_Ctype = (CComboBox*)GetDlgItem(IDC_COMBO_TYPECODE);
	CListCtrl* p_Ltype = (CListCtrl*)GetDlgItem(IDC_LIST_MEPTYPE);
	index_MEP = p_Cmep->GetCurSel();
	index_cate = p_Ccate->GetCurSel();
	POSITION pos = p_Ltype->GetFirstSelectedItemPosition();
	if (pos == NULL)
	{
		AfxMessageBox(L"未添加任何词条！");
	}
	while (pos)
	{
		sele_type.push_back(p_Ltype->GetNextSelectedItem(pos));
	}

	CDialog::OnOK();
}



void CAddMEPDlg::OnSelchangeComboCatecode()
{
	// TODO: 在此添加控件通知处理程序代码
	CComboBox* p_Ccate = (CComboBox*)GetDlgItem(IDC_COMBO_CATECODE);
	CListCtrl* p_Ltype = (CListCtrl*)GetDlgItem(IDC_LIST_MEPTYPE);
	p_Ltype->DeleteAllItems();
	index_cate = p_Ccate->GetCurSel();
	list<list<CString>>::iterator it = type.begin();
	advance(it,index_cate);
	list<CString> temp = *it;
	for (list<CString>::reverse_iterator rit = temp.rbegin(); rit != temp.rend(); ++ rit)
	{
		p_Ltype->InsertItem(0,*rit);
	}
}
