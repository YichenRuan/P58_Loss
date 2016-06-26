// CompDlg.cpp : 实现文件
//

#include "stdafx.h"
#include "PGCreator.h"
#include "CompDlg.h"
#include "afxdialogex.h"

CString compName[NUM_COMP] = {L"钢支撑刚架",L"钢结构梁柱节点",L"混凝土梁柱节点",L"混凝土连梁",L"混凝土剪力墙",L"无梁楼盖",
			L"砌体墙",L"玻璃幕墙",L"店面",L"屋顶",L"石膏板隔墙",L"楼梯",L"墙面装饰",L"天花板",L"吊顶灯",
			L"水管",L"冷却器", L"冷却塔",L"压缩器", L"管道风机",L"风管",L"变风箱", L"普通风机",L"散流器", L"空气处理单元", 
			L"控制面板", L"消防喷淋", L"变压器", L"电机控制中心", L"低压开关", L"配电板",L"电池架", L"充电器", L"柴油发电机"};

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
	ON_BN_CLICKED(IDC_BUTTON_UNCHECKALL, &CCompDlg::OnClickedButtonUncheckall)
	ON_BN_CLICKED(IDC_BUTTON_CHECKALL, &CCompDlg::OnClickedButtonCheckall)
	ON_BN_CLICKED(IDC_BUTTON_MEPCOMBO, &CCompDlg::OnClickedButtonMepcombo)
	ON_BN_CLICKED(IDC_BUTTON_ARCHCOMBO, &CCompDlg::OnClickedButtonArchcombo)
	ON_BN_CLICKED(IDC_BUTTON_STRUCOMBO, &CCompDlg::OnClickedButtonStrucombo)
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

	p_Lcomp->GetHeaderCtrl()->EnableWindow(FALSE);

	for (int i=NUM_COMP-1;0<=i;--i)
	{
		p_Lcomp->InsertItem(0,L" ");
		p_Lcomp->SetItemText(0,1,compName[i]);
	}

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

void CCompDlg::OnClickedButtonUncheckall()
{
	// TODO: 在此添加控件通知处理程序代码
	CListCtrl* p_Lcomp = (CListCtrl*)GetDlgItem(IDC_LIST_COMP);
	for (int i = NUM_COMP - 1; 0 <= i; --i)
	{
		p_Lcomp->SetCheck(i,FALSE);
	}
}


void CCompDlg::OnClickedButtonCheckall()
{
	// TODO: 在此添加控件通知处理程序代码
	CListCtrl* p_Lcomp = (CListCtrl*)GetDlgItem(IDC_LIST_COMP);
	for (int i = NUM_COMP - 1; 0 <= i; --i)
	{
		p_Lcomp->SetCheck(i,TRUE);
	}
}


void CCompDlg::OnClickedButtonMepcombo()
{
	// TODO: 在此添加控件通知处理程序代码
	CListCtrl* p_Lcomp = (CListCtrl*)GetDlgItem(IDC_LIST_COMP);
	for (int i = NUM_COMP - 1; 15 <= i; --i)
	{
		p_Lcomp->SetCheck(i,TRUE);
	}
	for (int i = 14; 0 <= i; --i)
	{
		p_Lcomp->SetCheck(i,FALSE);
	}
}


void CCompDlg::OnClickedButtonArchcombo()
{
	// TODO: 在此添加控件通知处理程序代码
	CListCtrl* p_Lcomp = (CListCtrl*)GetDlgItem(IDC_LIST_COMP);
	for (int i = NUM_COMP - 1; 15 <= i; --i)
	{
		p_Lcomp->SetCheck(i, FALSE);
	}
	for (int i = 14; 6 <= i; --i)
	{
		p_Lcomp->SetCheck(i, TRUE);
	}
	for (int i = 5; 0 <= i; --i)
	{
		p_Lcomp->SetCheck(i, FALSE);
	}
}


void CCompDlg::OnClickedButtonStrucombo()
{
	// TODO: 在此添加控件通知处理程序代码
	CListCtrl* p_Lcomp = (CListCtrl*)GetDlgItem(IDC_LIST_COMP);
	for (int i = NUM_COMP - 1; 6 <= i; --i)
	{
		p_Lcomp->SetCheck(i, FALSE);
	}
	for (int i = 5; 0 <= i; --i)
	{
		p_Lcomp->SetCheck(i, TRUE);
	}
}
