// MEPDlg.cpp : ʵ���ļ�
//

#include "stdafx.h"
#include "PGCreator.h"
#include "MEPDlg.h"
#include "afxdialogex.h"
using namespace std;

// CMEPDlg �Ի���

IMPLEMENT_DYNAMIC(CMEPDlg, CDialog)

CMEPDlg::CMEPDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CMEPDlg::IDD, pParent)
{
	MEPComp.push_back(L"��ȴ��");
	MEPComp.push_back(L"��ȴ��");
	MEPComp.push_back(L"ѹ����");
	MEPComp.push_back(L"�ܵ����");
	MEPComp.push_back(L"�����");
	MEPComp.push_back(L"��ͨ���");
	MEPComp.push_back(L"ɢ����");
	MEPComp.push_back(L"��������Ԫ");
	MEPComp.push_back(L"�������");
	MEPComp.push_back(L"��������");
	MEPComp.push_back(L"��ѹ��");
	MEPComp.push_back(L"�����������");
	MEPComp.push_back(L"��ѹ����");
	MEPComp.push_back(L"����");
	MEPComp.push_back(L"��ؼ�");
	MEPComp.push_back(L"�����");
	MEPComp.push_back(L"���ͷ����");
	num_entry = 0;
}

CMEPDlg::~CMEPDlg()
{
}

void CMEPDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CMEPDlg, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_ADD, &CMEPDlg::OnClickedButtonAdd)
	ON_BN_CLICKED(IDC_BUTTON_DELETE, &CMEPDlg::OnClickedButtonDelete)
END_MESSAGE_MAP()


// CMEPDlg ��Ϣ�������


BOOL CMEPDlg::OnInitDialog()
{
	CListCtrl* p_Lmep = (CListCtrl*)GetDlgItem(IDC_LIST_MEP);
	DWORD dwStyle = p_Lmep->GetExtendedStyle();
	dwStyle |= LVS_EX_FULLROWSELECT;
	dwStyle |= LVS_EX_GRIDLINES;

	p_Lmep->SetExtendedStyle(dwStyle); 
	p_Lmep->InsertColumn(0,L"empty", LVCFMT_CENTER,90);
	p_Lmep->InsertColumn(1,L"MEP����", LVCFMT_CENTER,108);
	p_Lmep->InsertColumn(2,L"Category", LVCFMT_CENTER,108);
	p_Lmep->InsertColumn(3,L"Type", LVCFMT_CENTER,198);
	p_Lmep->DeleteColumn(0);
	//p_Lmep->GetHeaderCtrl()->EnableWindow(FALSE);
	return TRUE;
}


void CMEPDlg::OnClickedButtonAdd()
{
	// TODO: �ڴ���ӿؼ�֪ͨ����������
	if (cate.size() == 0)
	{
		AfxMessageBox(L"��Ŀ��δ�ҵ�MEP����");
		return;
	}
	else if (m_addMEPDlg.DoModal() == IDOK)
	{
		AddItem();
	}
}


BOOL CMEPDlg::PreTranslateMessage(MSG* pMsg)
{
	// TODO: �ڴ����ר�ô����/����û���
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_ESCAPE ) return TRUE;
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_RETURN ) return TRUE;
	else return CDialog::PreTranslateMessage(pMsg);
}

void CMEPDlg::InFileInterpret(char* inFile, int startPoint)
{
	int i = startPoint;
	while (inFile[i] != '\n') ++i;
	inFile[i] = '\0';
	int num = atoi(inFile + startPoint);
	int hot = ++i;
	for (int j = 0; j < num; ++j)
	{
		list<CString> temp;
		while(inFile[i] != '\n')
		{
			if (inFile[i] == '\t')
			{
				inFile[i] = '\0';
				temp.push_back(CString(inFile + hot));
				hot = i + 1;
			}
			++i;
		}
		cate.push_back(temp.front());
		temp.pop_front();
		type.push_back(temp);
		hot = ++i;
	}
	m_addMEPDlg.Synchronize(MEPComp, cate, type);
}

void CMEPDlg::AddItem()
{
	/*
	MEPNode mepNode;
	mepNode.MEPCode = m_addMEPDlg.index_MEP;
	mepNode.Cate = m_addMEPDlg.index_cate;
	mepNode.Type = m_addMEPDlg.index_type;
	for (list<MEPNode>::iterator it = nodes.begin(); it != nodes.end(); ++it)
	{
		if ((*it).MEPCode == mepNode.MEPCode && (*it).Cate == mepNode.Cate && (*it).Type == mepNode.Type)
		{
			AfxMessageBox(L"�����Ѵ��ڣ�");
			return;
		}
	}
	nodes.push_back(mepNode);
	
	CListCtrl* p_Lmep = (CListCtrl*)GetDlgItem(IDC_LIST_MEP);
	list<CString>::iterator it1 = MEPComp.begin();
	advance(it1,m_addMEPDlg.index_MEP);
	list<CString>::iterator it2 = cate.begin();
	advance(it2,m_addMEPDlg.index_cate);
	list<list<CString>>::iterator it3 = type.begin();
	advance(it3,m_addMEPDlg.index_cate);
	list<CString>::iterator it4 = (*it3).begin();
	advance(it4,m_addMEPDlg.index_type);

	p_Lmep->InsertItem(num_entry,*it1);
	p_Lmep->SetItemText(num_entry,1,*it2);
	p_Lmep->SetItemText(num_entry,2,*it4);
	++num_entry;
	*/
	if (m_addMEPDlg.sele_type.empty())	return;
	else
	{
		int index_MEP = m_addMEPDlg.index_MEP;
		int index_cate = m_addMEPDlg.index_cate;
		int index_type;
		CListCtrl* p_Lmep = (CListCtrl*)GetDlgItem(IDC_LIST_MEP);
		list<CString>::iterator it1 = MEPComp.begin();
		advance(it1,index_MEP);
		list<CString>::iterator it2 = cate.begin();
		advance(it2,index_cate);
		list<list<CString>>::iterator it3 = type.begin();
		advance(it3,index_cate);
		bool isEverFound = false;

		for (list<int>::iterator offset = m_addMEPDlg.sele_type.begin(); offset != m_addMEPDlg.sele_type.end(); ++offset)
		{
			index_type = *offset;
			bool isFound = false;
			for (list<MEPNode>::iterator it = nodes.begin(); it != nodes.end(); ++it)
			{
				if ((*it).MEPCode == index_MEP && (*it).Cate == index_cate && (*it).Type == index_type)
				{
					isFound = true;
					break;
				}
			}
			if (!isFound)
			{
				MEPNode mepNode;
				mepNode.MEPCode = index_MEP;
				mepNode.Cate = index_cate;
				mepNode.Type = index_type;
				nodes.push_back(mepNode);
				list<CString>::iterator it4 = (*it3).begin();
				advance(it4,index_type);
				p_Lmep->InsertItem(num_entry,*it1);
				p_Lmep->SetItemText(num_entry,1,*it2);
				p_Lmep->SetItemText(num_entry,2,*it4);
				++num_entry;
			}
			else
			{
				isEverFound = true;
			}
		}
		if (isEverFound)
		{
			AfxMessageBox(L"�����ظ��������Ѻϲ�");
		}
	}
}




void CMEPDlg::OnClickedButtonDelete()
{
	// TODO: �ڴ���ӿؼ�֪ͨ����������
	CListCtrl* p_Lmep = (CListCtrl*)GetDlgItem(IDC_LIST_MEP);
	int currsele = p_Lmep->GetSelectionMark();
	if (currsele != -1)
	{
		p_Lmep->DeleteItem(currsele);
		list<MEPNode>::iterator it = nodes.begin();
		advance(it,currsele);
		nodes.erase(it);
		currsele = p_Lmep->GetSelectionMark();
		p_Lmep->SetItemState(currsele, LVIS_SELECTED|LVIS_FOCUSED, LVIS_SELECTED|LVIS_FOCUSED);
		--num_entry;
	}
}

void CMEPDlg::OutputInfo(FILE* fp_b)
{
	for (list<MEPNode>::iterator it = nodes.begin(); it != nodes.end(); ++it)
	{
		fwrite(&(*it),sizeof(MEPNode),1,fp_b);
	}
}