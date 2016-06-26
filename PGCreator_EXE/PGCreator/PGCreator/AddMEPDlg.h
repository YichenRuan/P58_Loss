#pragma once
#include <list>
using namespace std;

// CAddMEPDlg �Ի���

class CAddMEPDlg : public CDialog
{
	DECLARE_DYNAMIC(CAddMEPDlg)

public:
	CAddMEPDlg(CWnd* pParent = NULL);   // ��׼���캯��
	virtual ~CAddMEPDlg();

// �Ի�������
	enum { IDD = IDD_ADDMEP_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV ֧��

public:
	list<CString> MEPComp;
	list<CString> cate;
	list<list<CString>> type;

	DECLARE_MESSAGE_MAP()
public:
	int index_MEP, index_cate;
	list<int> sele_type;
	virtual BOOL OnInitDialog();
	void Synchronize(list<CString>& MEPComp0, list<CString>& cate0, list<list<CString>>& type0);
	afx_msg void OnBnClickedOk();
	afx_msg void OnSelchangeComboCatecode();
};
