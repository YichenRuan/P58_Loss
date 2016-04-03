// FireProDlg.cpp : 实现文件
//

#include "stdafx.h"
#include "PGCreator.h"
#include "FireProDlg.h"
#include "afxdialogex.h"
#include <list>


// CFireProDlg 对话框

IMPLEMENT_DYNAMIC(CFireProDlg, CDialog)

CFireProDlg::CFireProDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CFireProDlg::IDD, pParent)
{
	currCate = currUp = currDown = -1;
}

CFireProDlg::~CFireProDlg()
{
}

void CFireProDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CFireProDlg, CDialog)
	ON_NOTIFY(NM_CLICK, IDC_LIST_CATE, &CFireProDlg::OnClickListCate)
	ON_NOTIFY(NM_CLICK, IDC_LIST_UP, &CFireProDlg::OnClickListUp)
	ON_NOTIFY(NM_KILLFOCUS, IDC_LIST_UP, &CFireProDlg::OnKillfocusListUp)
	ON_NOTIFY(NM_CLICK, IDC_LIST_DOWN, &CFireProDlg::OnClickListDown)
	ON_NOTIFY(NM_KILLFOCUS, IDC_LIST_DOWN, &CFireProDlg::OnKillfocusListDown)
	ON_BN_CLICKED(IDC_BUTTON_GODOWN, &CFireProDlg::OnClickedButtonGodown)
	ON_BN_CLICKED(IDC_BUTTON_GOUP, &CFireProDlg::OnClickedButtonGoup)
	ON_BN_CLICKED(IDC_BUTTON_ADDALL, &CFireProDlg::OnClickedButtonAddall)
	ON_BN_CLICKED(IDC_BUTTON_DELEALL, &CFireProDlg::OnClickedButtonDeleall)
END_MESSAGE_MAP()


// CFireProDlg 消息处理程序

void CFireProDlg::In2FileInterpret(char* in2File)
{
	CListCtrl* p_Lcate = (CListCtrl*)GetDlgItem(IDC_LIST_CATE);
	CListCtrl* p_Lup = (CListCtrl*)GetDlgItem(IDC_LIST_UP);
	CListCtrl* p_Ldown = (CListCtrl*)GetDlgItem(IDC_LIST_DOWN);
	num_cate = in2File[0] - '0';
	if (num_cate == 0)	return;
	up = new std::list<FPCombo>[num_cate];
	down = new std::list<FPCombo>[num_cate];
	int posi = 2;
	int hot = posi;
	for (int i=0;i<num_cate;++i)
	{
		int count = -1;
		while (in2File[posi] != '\n')
		{
			if (in2File[posi] == '\t')
			{
				in2File[posi] = '\0';
				up[i].push_back(FPCombo(count++,(CString)(in2File + hot)));
				hot = posi + 1;
			}
			++posi;
		}
		p_Lcate->InsertItem(i,up[i].front().name);
		up[i].pop_front();
		hot = ++posi;
	}
}

BOOL CFireProDlg::OnInitDialog()
{
	CListCtrl* p_Lcate = (CListCtrl*)GetDlgItem(IDC_LIST_CATE);
	CListCtrl* p_Lup = (CListCtrl*)GetDlgItem(IDC_LIST_UP);
	CListCtrl* p_Ldown = (CListCtrl*)GetDlgItem(IDC_LIST_DOWN);
	DWORD dwStyle = p_Lcate->GetExtendedStyle();
	dwStyle |= LVS_EX_FULLROWSELECT;
	dwStyle |= LVS_EX_GRIDLINES;
	p_Lcate->SetExtendedStyle(dwStyle); 
	p_Lup->SetExtendedStyle(dwStyle); 
	p_Ldown->SetExtendedStyle(dwStyle);
	p_Lcate->InsertColumn(0,L"empty", LVCFMT_CENTER,50);
	p_Lcate->InsertColumn(1,L"族类型", LVCFMT_CENTER,107);
	p_Lcate->DeleteColumn(0);
	p_Lup->InsertColumn(0,L"empty", LVCFMT_CENTER,50);
	p_Lup->InsertColumn(1,L"指定族", LVCFMT_CENTER,179);
	p_Lup->DeleteColumn(0);
	p_Ldown->InsertColumn(0,L"empty", LVCFMT_CENTER,50);
	p_Ldown->InsertColumn(1,L"未指定族", LVCFMT_CENTER,180);
	p_Ldown->DeleteColumn(0);

	p_Lcate->GetHeaderCtrl()->EnableWindow(FALSE);
	p_Lup->GetHeaderCtrl()->EnableWindow(FALSE);
	p_Ldown->GetHeaderCtrl()->EnableWindow(FALSE);

	return TRUE;
}



void CFireProDlg::OnClickListCate(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMITEMACTIVATE pNMItemActivate = reinterpret_cast<LPNMITEMACTIVATE>(pNMHDR);
	
	CListCtrl* p_Llevel = (CListCtrl*)GetDlgItem(IDC_LIST_LEVEL);
	CListCtrl* p_Lup = (CListCtrl*)GetDlgItem(IDC_LIST_UP);
	CListCtrl* p_Ldown = (CListCtrl*)GetDlgItem(IDC_LIST_DOWN);
	p_Lup->DeleteAllItems();
	p_Ldown->DeleteAllItems();
	currCate = pNMItemActivate->iItem;
	std::list<FPCombo>::iterator iter;
	int count = 0;
	if (currCate == -1)	return;
	else
	{
		currUp = -1;
		currDown = -1;
		for (iter = up[currCate].begin();iter != up[currCate].end();++iter)
		{
			p_Lup->InsertItem(count++, iter->name);
		}
		count = 0;
		for (iter = down[currCate].begin();iter != down[currCate].end();++iter)
		{
			p_Ldown->InsertItem(count++, iter->name);
		}
	}
}

void CFireProDlg::OnClickListUp(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMITEMACTIVATE pNMItemActivate = reinterpret_cast<LPNMITEMACTIVATE>(pNMHDR);
	currUp = pNMItemActivate->iItem;
}


void CFireProDlg::OnKillfocusListUp(NMHDR *pNMHDR, LRESULT *pResult)
{
	// TODO: 在此添加控件通知处理程序代码
	CWnd *pWnd= GetFocus();
	int focusID = pWnd ->GetDlgCtrlID();
	if (focusID != IDC_BUTTON_GODOWN && focusID != IDC_BUTTON_DELEALL)
	{
		currUp = -1;
	}
}


void CFireProDlg::OnClickListDown(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMITEMACTIVATE pNMItemActivate = reinterpret_cast<LPNMITEMACTIVATE>(pNMHDR);
	currDown = pNMItemActivate->iItem;
}


void CFireProDlg::OnKillfocusListDown(NMHDR *pNMHDR, LRESULT *pResult)
{
	// TODO: 在此添加控件通知处理程序代码
	CWnd *pWnd= GetFocus();
	int focusID = pWnd ->GetDlgCtrlID();
	if (focusID != IDC_BUTTON_GOUP && focusID != IDC_BUTTON_ADDALL)
	{
		currDown = -1;
	}
}


void CFireProDlg::OnClickedButtonGodown()
{
	// TODO: 在此添加控件通知处理程序代码
	if (currCate != -1 && currUp != -1 && !up[currCate].empty())
	{
		CListCtrl* p_Lup = (CListCtrl*)GetDlgItem(IDC_LIST_UP);
		CListCtrl* p_Ldown = (CListCtrl*)GetDlgItem(IDC_LIST_DOWN);
		std::list<FPCombo>::iterator iter = up[currCate].begin();
		advance(iter,currUp);
		down[currCate].push_back(*iter);
		up[currCate].erase(iter);

		p_Lup->DeleteItem(currUp);
		if (up[currCate].empty())
		{
			currUp = -1;
		}
		else
		{
			--currUp;
			if (currUp == -1)	currUp = 0;
			p_Lup->SetItemState(currUp, LVIS_SELECTED|LVIS_FOCUSED, LVIS_SELECTED|LVIS_FOCUSED);
		}
		p_Ldown->InsertItem(p_Ldown->GetItemCount(), down[currCate].back().name);
	}
}


void CFireProDlg::OnClickedButtonGoup()
{
	// TODO: 在此添加控件通知处理程序代码
	if (currCate != -1 && currDown != -1 && !down[currCate].empty())
	{
		CListCtrl* p_Lup = (CListCtrl*)GetDlgItem(IDC_LIST_UP);
		CListCtrl* p_Ldown = (CListCtrl*)GetDlgItem(IDC_LIST_DOWN);
		std::list<FPCombo>::iterator iter = down[currCate].begin();
		advance(iter,currDown);
		up[currCate].push_back(*iter);
		down[currCate].erase(iter);

		p_Ldown->DeleteItem(currDown);
		if (down[currCate].empty())
		{
			currDown = -1;
		}
		else
		{
			--currDown;
			if (currDown == -1)	currDown = 0;
			p_Ldown->SetItemState(currDown, LVIS_SELECTED|LVIS_FOCUSED, LVIS_SELECTED|LVIS_FOCUSED);
		}
		p_Lup->InsertItem(p_Lup->GetItemCount(), up[currCate].back().name);
	}
}


void CFireProDlg::OnClickedButtonAddall()
{
	// TODO: 在此添加控件通知处理程序代码
	if (currCate != -1 && !down[currCate].empty())
	{
		CListCtrl* p_Lup = (CListCtrl*)GetDlgItem(IDC_LIST_UP);
		CListCtrl* p_Ldown = (CListCtrl*)GetDlgItem(IDC_LIST_DOWN);
		std::list<FPCombo>::iterator iter;
		p_Ldown->DeleteAllItems();
		for (iter = down[currCate].begin();iter != down[currCate].end();++iter)
		{
			up[currCate].push_back(*iter);
			p_Lup->InsertItem(p_Lup->GetItemCount(),iter->name);
		}
		down[currCate].clear();
	}
}


void CFireProDlg::OnClickedButtonDeleall()
{
	// TODO: 在此添加控件通知处理程序代码
	if (currCate != -1 && !up[currCate].empty())
	{
		CListCtrl* p_Lup = (CListCtrl*)GetDlgItem(IDC_LIST_UP);
		CListCtrl* p_Ldown = (CListCtrl*)GetDlgItem(IDC_LIST_DOWN);
		std::list<FPCombo>::iterator iter;
		p_Lup->DeleteAllItems();
		for (iter = up[currCate].begin();iter != up[currCate].end();++iter)
		{
			down[currCate].push_back(*iter);
			p_Ldown->InsertItem(p_Ldown->GetItemCount(),iter->name);
		}
		up[currCate].clear();
	}
}

void CFireProDlg::OutputInfo(FILE* fp)
{
	for (int i=0;i<num_cate;++i)
	{
		//fprintf_s(fp,"%d\t",i);
		std::list<FPCombo>::iterator iter;
		for (iter = up[i].begin();iter != up[i].end();++iter)
		{
			fprintf_s(fp,"%d\t",iter->index);
		}
		fprintf_s(fp,"\n");
	}
}

BOOL CFireProDlg::PreTranslateMessage(MSG* pMsg)
{
	// TODO: 在此添加专用代码和/或调用基类
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_ESCAPE ) return TRUE;
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_RETURN ) return TRUE;
	else return CDialog::PreTranslateMessage(pMsg);
}
