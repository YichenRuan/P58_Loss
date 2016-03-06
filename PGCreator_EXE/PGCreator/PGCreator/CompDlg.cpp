// CompDlg.cpp : 实现文件
//

#include "stdafx.h"
#include "PGCreator.h"
#include "CompDlg.h"
#include "afxdialogex.h"


// CCompDlg 对话框

IMPLEMENT_DYNAMIC(CCompDlg, CDialog)

CCompDlg::CCompDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CCompDlg::IDD, pParent)
	, d_Eprice(0)
{

}

CCompDlg::~CCompDlg()
{
}

void CCompDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_PRICE, d_Eprice);
}


BEGIN_MESSAGE_MAP(CCompDlg, CDialog)
	ON_NOTIFY(NM_CLICK, IDC_LIST_COMP, &CCompDlg::OnClickListComp)
	ON_EN_KILLFOCUS(IDC_EDIT_PRICE, &CCompDlg::OnKillfocusEditPrice)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_COMP, &CCompDlg::OnItemchangedListComp)
END_MESSAGE_MAP()


// CCompDlg 消息处理程序


BOOL CCompDlg::PreTranslateMessage(MSG* pMsg)
{
	// TODO: 在此添加专用代码和/或调用基类
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_ESCAPE ) return TRUE;
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_RETURN ) return TRUE;
	else return CDialog::PreTranslateMessage(pMsg);
}


BOOL CCompDlg::OnInitDialog()
{
	CListCtrl* p_Lcomp = (CListCtrl*)GetDlgItem(IDC_LIST_COMP);
	CEdit* p_Eprice = (CEdit*)GetDlgItem(IDC_EDIT_PRICE);
	DWORD dwStyle = p_Lcomp->GetExtendedStyle();
	dwStyle |= LVS_EX_FULLROWSELECT;
	dwStyle |= LVS_EX_GRIDLINES;
	dwStyle |= LVS_EX_CHECKBOXES;
	//dwStyle |= LVS_SHOWSELALWAYS;
	p_Lcomp->SetExtendedStyle(dwStyle); 
	p_Lcomp->InsertColumn(0,L"empty", LVCFMT_CENTER,90);
	p_Lcomp->InsertColumn(1,L" ", LVCFMT_CENTER,22);
	p_Lcomp->InsertColumn(2,L"构件名", LVCFMT_CENTER,187);
	p_Lcomp->InsertColumn(3,L"价格 (美元)", LVCFMT_CENTER,188);
	p_Lcomp->DeleteColumn(0);

	p_Lcomp->InsertItem(0,L" ");
	p_Lcomp->SetItemText(0,1,L"梁柱节点");
	p_Lcomp->InsertItem(1,L" ");
	p_Lcomp->SetItemText(1,1,L"剪力墙");
	p_Lcomp->InsertItem(2,L" ");
	p_Lcomp->SetItemText(2,1,L"石膏板隔墙");
	p_Lcomp->InsertItem(3,L" ");
	p_Lcomp->SetItemText(3,1,L"玻璃幕墙");
	p_Lcomp->InsertItem(4,L" ");
	p_Lcomp->SetItemText(4,1,L"店面");

	for (int i=0;i<NUM_COMP;++i)
	{
		price[i] = 0.0;
		p_Lcomp->SetCheck(i,TRUE);
	}

	p_Eprice->ShowWindow(SW_HIDE);

	return TRUE;
}




void CCompDlg::OnClickListComp(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMITEMACTIVATE pNMItemActivate = reinterpret_cast<LPNMITEMACTIVATE>(pNMHDR);

	// TODO: 在此添加控件通知处理程序代码
	*pResult = 0;
	CListCtrl* p_Lcomp = (CListCtrl*)GetDlgItem(IDC_LIST_COMP);
	CEdit* p_Eprice = (CEdit*)GetDlgItem(IDC_EDIT_PRICE);
	currItem = pNMItemActivate->iItem;
	currSubItem = pNMItemActivate->iSubItem;
	if (p_Lcomp->GetCheck(currItem))
	{
		if (currItem == -1 || currSubItem != 2)	return;
		CRect rect_dlg,rect_list,rect_edit;
		POINT point;
		GetWindowRect(rect_dlg);
		p_Lcomp->GetWindowRect(rect_list);
		p_Lcomp->GetSubItemRect(currItem, currSubItem, LVIR_LABEL, rect_edit);
		point.x = rect_list.left - rect_dlg.left;
		point.y = rect_list.top - rect_dlg.top;
		rect_edit.OffsetRect(point);
		rect_edit.bottom += 2;
		p_Eprice->ShowWindow(SW_SHOW);
		p_Eprice->MoveWindow(&rect_edit, TRUE);
		p_Eprice->SetFocus();
		p_Lcomp->SetItemText(currItem,currSubItem,L" ");
	}
}



void CCompDlg::OnKillfocusEditPrice()
{
	// TODO: 在此添加控件通知处理程序代码
	CListCtrl* p_Lcomp = (CListCtrl*)GetDlgItem(IDC_LIST_COMP);
	CEdit* p_Eprice = (CEdit*)GetDlgItem(IDC_EDIT_PRICE);
	CString temp;
	p_Eprice->ShowWindow(SW_HIDE);
	p_Eprice->GetWindowTextW(temp);
	if (!temp.IsEmpty())
	{
		if (UpdateData(TRUE))
		{
			price[currItem] = d_Eprice;
		}
	}
	if (0 < price[currItem])
	{
		temp.Format(L"%.2f",price[currItem]);
		p_Lcomp->SetItemText(currItem,currSubItem,temp);
	}
	else
	{
		p_Lcomp->SetItemText(currItem,currSubItem,L"默认");
	}
	p_Eprice->SetWindowTextW(L"\0");
}


void CCompDlg::OnItemchangedListComp(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);
	// TODO: 在此添加控件通知处理程序代码
	*pResult = 0;
	CListCtrl* p_Lcomp = (CListCtrl*)GetDlgItem(IDC_LIST_COMP);
	int nRow = pNMLV->iItem;
	if((pNMLV->uOldState & INDEXTOSTATEIMAGEMASK(1))		/* old state : unchecked */ 
		&& (pNMLV->uNewState & INDEXTOSTATEIMAGEMASK(2)))	/* new state : checked */ 
	{
		price[nRow] = 0.0;
		p_Lcomp->SetItemText(nRow,2,L"默认");
	}
	else if((pNMLV->uOldState & INDEXTOSTATEIMAGEMASK(2))	/* old state : checked */ 
		&& (pNMLV->uNewState & INDEXTOSTATEIMAGEMASK(1)))	/* new state : unchecked */ 
	{
		price[nRow] = -1.0;
		p_Lcomp->SetItemText(nRow,2,L" ");
	}
}

void CCompDlg::OutputInfo(FILE* fp)
{
	//in price[], -1.0: unchecked, 0.0:default, positive:user defined price
	for (int i=0;i<NUM_COMP;++i)
	{
		if (0.0 <= price[i])
		{
			fprintf_s(fp,"%d\t%.2f\t",i,price[i]);
		}
	}
	fprintf_s(fp,"\n");
}